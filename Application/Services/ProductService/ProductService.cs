
using AutoMapper;
using FYP2025.Application.DTOs;
using FYP2025.Application.Services.ProductService;
using FYP2025.Domain.Entities;
using FYP2025.Domain.Repositories;
using FYP2025.Domain.Services.Cloudinary;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FYP2025.Application.Services.ProductService
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IPhotoService _photoService;
        private readonly IMapper _mapper;

        public ProductService(IProductRepository productRepository,
                              ICategoryRepository categoryRepository,
                              IPhotoService photoService,
                              IMapper mapper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _photoService = photoService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> GetProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto> GetProductByIdAsync(string id)
        {
            var product = await _productRepository.GetByIdAsync(id.Trim());
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryIdAsync(string categoryId)
        {
            var trimmedCategoryId = categoryId.Trim();
            if (!await _categoryRepository.ExistsAsync(trimmedCategoryId))
            {
                throw new Exception($"Category with ID {trimmedCategoryId} not found.");
            }

            var products = await _productRepository.GetProductsByCategoryIdAsync(trimmedCategoryId);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
        {
            if (!await _categoryRepository.ExistsAsync(createProductDto.CategoryId))
            {
                throw new Exception($"Danh mục với ID {createProductDto.CategoryId} không tồn tại.");
            }

            if (createProductDto.ImageFile == null || createProductDto.ImageFile.Length == 0)
            {
                throw new Exception("Cần có file ảnh cho sản phẩm chính.");
            }

            var productUploadResult = await _photoService.UploadPhotoAsync(createProductDto.ImageFile);
            if (productUploadResult.Error != null || productUploadResult.SecureUrl == null)
            {
                throw new Exception($"Tải ảnh sản phẩm chính lên thất bại: {productUploadResult?.Error?.Message ?? "Không nhận được URL ảnh."}");
            }

            if (createProductDto.Variants == null || !createProductDto.Variants.Any())
            {
                throw new Exception("Cần ít nhất một biến thể sản phẩm.");
            }

            var product = _mapper.Map<Product>(createProductDto);
            product.Id = Guid.NewGuid().ToString();
            product.ImageUrl = productUploadResult.SecureUrl.ToString();

            List<ProductVariant> finalVariants = new List<ProductVariant>();

            foreach (var variantDto in createProductDto.Variants)
            {
                if (variantDto.ImageFile == null || variantDto.ImageFile.Length == 0)
                {
                    throw new Exception($"Biến thể '{variantDto.Color} - {variantDto.Size}' yêu cầu file ảnh.");
                }
                var variantUploadResult = await _photoService.UploadPhotoAsync(variantDto.ImageFile);
                if (variantUploadResult.Error != null || variantUploadResult.SecureUrl == null)
                {
                    throw new Exception($"Tải ảnh lên thất bại cho biến thể '{variantDto.Color} - {variantDto.Size}': {variantUploadResult?.Error?.Message ?? "Không nhận được URL ảnh."}");
                }
                var variant = _mapper.Map<ProductVariant>(variantDto);
                variant.Id = Guid.NewGuid().ToString();
                variant.ProductId = product.Id;
                variant.ImageUrl = variantUploadResult.SecureUrl.ToString();

                finalVariants.Add(variant);
            }

            product.Variants = finalVariants;

            await _productRepository.AddAsync(product);

            var createdProduct = await _productRepository.GetByIdAsync(product.Id);
            if (createdProduct == null)
            {
                throw new Exception("Không thể lấy sản phẩm vừa tạo.");
            }

            return _mapper.Map<ProductDto>(createdProduct);
        }

        public async Task<ProductDto> UpdateProductAsync(string id, UpdateProductDto updateProductDto)
        {
            var trimmedId = id.Trim();
            var productToUpdate = await _productRepository.GetByIdAsync(trimmedId);

            if (productToUpdate == null)
                throw new Exception($"Product with ID {trimmedId} not found.");

            // Kiểm tra Category
            if (!await _categoryRepository.ExistsAsync(updateProductDto.CategoryId))
                throw new Exception($"Category with ID {updateProductDto.CategoryId} does not exist.");

            // --- Upload ảnh sản phẩm nếu có ---
            if (updateProductDto.ImageFile != null && updateProductDto.ImageFile.Length > 0)
            {
                // Xoá ảnh cũ nếu có
                if (!string.IsNullOrEmpty(productToUpdate.ImageUrl))
                {
                    try
                    {
                        var uri = new Uri(productToUpdate.ImageUrl);
                        var publicId = Path.GetFileNameWithoutExtension(uri.LocalPath);
                        await _photoService.DeletePhotoAsync(publicId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting old product image: {ex.Message}");
                    }
                }

                var uploadResult = await _photoService.UploadPhotoAsync(updateProductDto.ImageFile);
                if (uploadResult.Error != null)
                    throw new Exception($"Failed to upload new product image: {uploadResult.Error.Message}");

                productToUpdate.ImageUrl = uploadResult.SecureUrl.ToString();
            }

            // --- Cập nhật thông tin chung của sản phẩm ---
            _mapper.Map(updateProductDto, productToUpdate);
            productToUpdate.Id = trimmedId;

            // --- Xử lý variants (chỉ thêm hoặc update, KHÔNG xoá) ---
            if (updateProductDto.Variants != null)
            {
                foreach (var variantDto in updateProductDto.Variants)
                {
                    if (string.IsNullOrEmpty(variantDto.Id))
                    {
                        // 🔵 Thêm mới variant
                        var newVariant = _mapper.Map<ProductVariant>(variantDto);
                        newVariant.Id = Guid.NewGuid().ToString();
                        newVariant.ProductId = productToUpdate.Id;

                        // Nếu có ảnh thì upload
                        if (variantDto.ImageFile != null && variantDto.ImageFile.Length > 0)
                        {
                            var uploadResult = await _photoService.UploadPhotoAsync(variantDto.ImageFile);
                            if (uploadResult.Error != null)
                                throw new Exception($"Failed to upload variant image: {uploadResult.Error.Message}");
                            newVariant.ImageUrl = uploadResult.SecureUrl.ToString();
                        }
                        else
                        {
                            // Nếu không có ảnh thì gán ảnh mặc định
                            newVariant.ImageUrl = "https://res.cloudinary.com/demo/image/upload/no-image.jpg";
                        }

                        productToUpdate.Variants.Add(newVariant);
                    }
                    else
                    {
                        // 🟢 Update variant cũ
                        var existingVariant = productToUpdate.Variants.FirstOrDefault(v => v.Id == variantDto.Id);
                        if (existingVariant != null)
                        {
                            // Nếu có ảnh mới thì upload và xoá ảnh cũ
                            if (variantDto.ImageFile != null && variantDto.ImageFile.Length > 0)
                            {
                                if (!string.IsNullOrEmpty(existingVariant.ImageUrl))
                                {
                                    try
                                    {
                                        var uri = new Uri(existingVariant.ImageUrl);
                                        var publicId = Path.GetFileNameWithoutExtension(uri.LocalPath);
                                        await _photoService.DeletePhotoAsync(publicId);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error deleting old variant image: {ex.Message}");
                                    }
                                }

                                var uploadResult = await _photoService.UploadPhotoAsync(variantDto.ImageFile);
                                if (uploadResult.Error != null)
                                    throw new Exception($"Failed to upload new variant image: {uploadResult.Error.Message}");
                                existingVariant.ImageUrl = uploadResult.SecureUrl.ToString();
                            }

                            // Cập nhật các trường còn lại
                            _mapper.Map(variantDto, existingVariant);
                        }
                    }
                }
            }

            await _productRepository.UpdateAsync(productToUpdate);
            return _mapper.Map<ProductDto>(productToUpdate);
        }




        public async Task UpdateProductImageAsync(string id, IFormFile imageFile)
        {
            var product = await _productRepository.GetByIdAsync(id.Trim());
            if (product == null)
            {
                throw new Exception("Product not found");
            }

            if (imageFile == null || imageFile.Length == 0)
            {
                throw new Exception("Image file is required.");
            }

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                try
                {
                    var uri = new Uri(product.ImageUrl);
                    var publicId = Path.GetFileNameWithoutExtension(uri.LocalPath);
                    var deleteResult = await _photoService.DeletePhotoAsync(publicId);
                    if (deleteResult.Result != "ok")
                    {
                        Console.WriteLine($"Failed to delete old image with public ID {publicId}: {deleteResult.Error?.Message}");
                    }
                }
                catch (UriFormatException ex)
                {
                    Console.WriteLine($"Invalid main product ImageUrl format for deletion: {product.ImageUrl} - {ex.Message}");
                }
            }

            var uploadResult = await _photoService.UploadPhotoAsync(imageFile);
            if (uploadResult.Error != null)
            {
                throw new Exception($"Failed to upload new image: {uploadResult.Error.Message}");
            }
            product.ImageUrl = uploadResult.SecureUrl.ToString();

            await _productRepository.UpdateAsync(product);
        }

        public async Task DeleteProductAsync(string id)
        {
            var trimmedId = id.Trim();
            var product = await _productRepository.GetByIdAsync(trimmedId);
            if (product == null)
            {
                throw new Exception("Product not found");
            }

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                try
                {
                    var uri = new Uri(product.ImageUrl);
                    var publicId = Path.GetFileNameWithoutExtension(uri.LocalPath);
                    var deleteResult = await _photoService.DeletePhotoAsync(publicId);
                    if (deleteResult.Result != "ok")
                    {
                        Console.WriteLine($"Failed to delete main product image {publicId} during product deletion: {deleteResult.Error?.Message}");
                    }
                }
                catch (UriFormatException ex)
                {
                    Console.WriteLine($"Invalid main product ImageUrl format for deletion: {product.ImageUrl} - {ex.Message}");
                }
            }

            if (product.Variants != null)
            {
                foreach (var variant in product.Variants)
                {
                    if (!string.IsNullOrEmpty(variant.ImageUrl))
                    {
                        try
                        {
                            var uri = new Uri(variant.ImageUrl);
                            var publicId = Path.GetFileNameWithoutExtension(uri.LocalPath);
                            var deleteResult = await _photoService.DeletePhotoAsync(publicId);
                            if (deleteResult.Result != "ok")
                            {
                                Console.WriteLine($"Failed to delete variant image {publicId} during product deletion: {deleteResult.Error?.Message}");
                            }
                        }
                        catch (UriFormatException ex)
                        {
                            Console.WriteLine($"Invalid variant ImageUrl format for deletion: {variant.ImageUrl} - {ex.Message}");
                        }
                    }
                }
            }

            await _productRepository.DeleteAsync(trimmedId);
        }

        public async Task<IEnumerable<ProductVariantDto>> GetProductVariantsAsync(string productId)
        {
            var product = await _productRepository.GetByIdAsync(productId.Trim());
            if (product == null)
            {
                throw new Exception($"Product with ID {productId} not found.");
            }
            return _mapper.Map<IEnumerable<ProductVariantDto>>(product.Variants);
        }

        public async Task<ProductVariantDto> GetProductVariantAsync(string productId, string variantId)
        {
            var product = await _productRepository.GetByIdAsync(productId.Trim());
            if (product == null)
            {
                throw new Exception($"Product with ID {productId} not found.");
            }
            var variant = product.Variants.FirstOrDefault(v => v.Id == variantId.Trim());
            if (variant == null)
            {
                throw new Exception($"Product variant with ID {variantId} not found for product {productId}.");
            }
            return _mapper.Map<ProductVariantDto>(variant);
        }

        public async Task<ProductVariantDto> AddProductVariantAsync(string productId, CreateProductVariantDto createVariantDto)
        {
            var trimmedProductId = productId.Trim();
            var product = await _productRepository.GetByIdAsync(trimmedProductId);
            if (product == null)
            {
                throw new Exception($"Product with ID {trimmedProductId} not found.");
            }

            var newVariant = _mapper.Map<ProductVariant>(createVariantDto);
            newVariant.Id = Guid.NewGuid().ToString();
            newVariant.ProductId = trimmedProductId;

            product.Variants.Add(newVariant);

            await _productRepository.UpdateAsync(product);

            return _mapper.Map<ProductVariantDto>(newVariant);
        }

        public async Task UpdateProductVariantAsync(string productId, string variantId, UpdateProductVariantDto updateVariantDto)
        {
            var trimmedProductId = productId.Trim();
            var trimmedVariantId = variantId.Trim();
            var product = await _productRepository.GetByIdAsync(trimmedProductId);
            if (product == null)
            {
                throw new Exception($"Product with ID {trimmedProductId} not found.");
            }

            var existingVariant = product.Variants.FirstOrDefault(v => v.Id == trimmedVariantId);
            if (existingVariant == null)
            {
                throw new Exception($"Product variant with ID {trimmedVariantId} not found for product {trimmedProductId}.");
            }

            if (updateVariantDto.ImageFile != null && updateVariantDto.ImageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(existingVariant.ImageUrl))
                {
                    try
                    {
                        var uri = new Uri(existingVariant.ImageUrl);
                        var publicId = Path.GetFileNameWithoutExtension(uri.LocalPath);
                        await _photoService.DeletePhotoAsync(publicId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting old variant image: {ex.Message}");
                    }
                }

                var uploadResult = await _photoService.UploadPhotoAsync(updateVariantDto.ImageFile);
                if (uploadResult.Error != null)
                {
                    throw new Exception($"Failed to upload new variant image: {uploadResult.Error.Message}");
                }
                existingVariant.ImageUrl = uploadResult.SecureUrl.ToString();
            }

            _mapper.Map(updateVariantDto, existingVariant);
            existingVariant.Id = trimmedVariantId;
            existingVariant.ProductId = trimmedProductId;

            await _productRepository.UpdateAsync(product);
        }

        public async Task DeleteProductVariantAsync(string productId, string variantId)
        {
            var trimmedProductId = productId.Trim();
            var product = await _productRepository.GetByIdAsync(trimmedProductId);
            if (product == null)
            {
                throw new Exception($"Product with ID {trimmedProductId} not found.");
            }

            var variantToRemove = product.Variants.FirstOrDefault(v => v.Id == variantId.Trim());
            if (variantToRemove == null)
            {
                throw new Exception($"Product variant with ID {variantId} not found for product {trimmedProductId}.");
            }

            if (!string.IsNullOrEmpty(variantToRemove.ImageUrl))
            {
                try
                {
                    var uri = new Uri(variantToRemove.ImageUrl);
                    var publicId = Path.GetFileNameWithoutExtension(uri.LocalPath);
                    var deleteResult = await _photoService.DeletePhotoAsync(publicId);
                    if (deleteResult.Result != "ok")
                    {
                        Console.WriteLine($"Failed to delete variant image {publicId} during variant deletion: {deleteResult.Error?.Message}");
                    }
                }
                catch (UriFormatException ex)
                {
                    Console.WriteLine($"Invalid variant ImageUrl format for deletion: {variantToRemove.ImageUrl} - {ex.Message}");
                }
            }

            product.Variants.Remove(variantToRemove);
            await _productRepository.UpdateAsync(product);
        }
    }
}
