using FYP2025.Application.DTOs;
using FYP2025.Domain.Entities;
using FYP2025.Domain.Repositories;
using FYP2025.Domain.Services.Cloudinary;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; 
using System.IO;
using System;
using System.Linq; 

namespace FYP2025.Api.Features.Products
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IPhotoService _photoService;
        private readonly IMapper _mapper;

        public ProductsController(IProductRepository productRepository,
                                  ICategoryRepository categoryRepository,
                                  IPhotoService photoService,
                                  IMapper mapper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _photoService = photoService;
            _mapper = mapper;
        }

        // GET api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var products = await _productRepository.GetAllAsync();
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
            return Ok(productDtos);
        }

        // GET api/products/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(string id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            var productDto = _mapper.Map<ProductDto>(product);
            return Ok(productDto);
        }

        // GET api/products/byCategory/{categoryId}
        [HttpGet("byCategory/{categoryId}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategoryId(string categoryId)
        {
            if (!await _categoryRepository.ExistsAsync(categoryId))
            {
                return NotFound($"Category with ID {categoryId} not found.");
            }

            var products = await _productRepository.GetProductsByCategoryIdAsync(categoryId);
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
            return Ok(productDtos);
        }


        // POST api/products 
        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromForm] CreateProductDto createProductDto)
        {
            // --- Debugging: Kiểm tra ProductImageFile và Variants được nhận ---
            Console.WriteLine($"--- Debug: Trong CreateProduct ---");
            Console.WriteLine($"ProductImageFile có null không: {createProductDto.ImageFile == null}");
            if (createProductDto.ImageFile != null)
            {
                Console.WriteLine($"Tên ProductImageFile: {createProductDto.ImageFile.FileName}");
                Console.WriteLine($"Kích thước ProductImageFile: {createProductDto.ImageFile.Length}");
            }
            else
            {
                Console.WriteLine($"ProductImageFile KHÔNG được nhận từ dữ liệu form.");
            }

            Console.WriteLine($"Số lượng Variants được nhận: {createProductDto.Variants?.Count ?? 0}");
            if (createProductDto.Variants != null)
            {
                foreach (var variantDto in createProductDto.Variants)
                {
                    Console.WriteLine($"  - Variant: Color={variantDto.Color}, Size={variantDto.Size}, ImageFile is null: {variantDto.ImageFile == null}");
                    if (variantDto.ImageFile != null)
                    {
                        Console.WriteLine($"    Variant ImageFile Name: {variantDto.ImageFile.FileName}");
                    }
                }
            }
            Console.WriteLine($"--- Kết thúc Debug ---");

            if (!await _categoryRepository.ExistsAsync(createProductDto.CategoryId))
            {
                return BadRequest($"Danh mục với ID {createProductDto.CategoryId} không tồn tại.");
            }

            if (createProductDto.ImageFile == null || createProductDto.ImageFile.Length == 0)
            {
                return BadRequest("Cần có file ảnh cho sản phẩm chính.");
            }

            var productUploadResult = await _photoService.UploadPhotoAsync(createProductDto.ImageFile);
            if (productUploadResult.Error != null || productUploadResult.SecureUrl == null)
            {
                Console.WriteLine($"Tải ảnh sản phẩm chính lên Cloudinary thất bại: {productUploadResult?.Error?.Message ?? "URL an toàn là null/rỗng"}");
                return StatusCode(500, $"Tải ảnh sản phẩm chính lên thất bại: {productUploadResult?.Error?.Message ?? "Không nhận được URL ảnh."}");
            }

            if (createProductDto.Variants == null || !createProductDto.Variants.Any())
            {
                return BadRequest("Cần ít nhất một biến thể sản phẩm.");
            }

            var product = _mapper.Map<Product>(createProductDto);
            product.Id = Guid.NewGuid().ToString();
            product.ImageUrl = productUploadResult.SecureUrl.ToString(); 


            List<ProductVariant> finalVariants = new List<ProductVariant>();

            foreach (var variantDto in createProductDto.Variants) 
            {
                if (variantDto.ImageFile == null || variantDto.ImageFile.Length == 0)
                {
                    return BadRequest($"Biến thể '{variantDto.Color} - {variantDto.Size}' yêu cầu file ảnh.");
                }
                var variantUploadResult = await _photoService.UploadPhotoAsync(variantDto.ImageFile);
                if (variantUploadResult.Error != null || variantUploadResult.SecureUrl == null)
                {
                    Console.WriteLine($"Tải ảnh lên Cloudinary thất bại cho biến thể '{variantDto.Color} - {variantDto.Size}': {variantUploadResult?.Error?.Message ?? "URL an toàn là null/rỗng"}");
                    return StatusCode(500, $"Tải ảnh lên thất bại cho biến thể '{variantDto.Color} - {variantDto.Size}': {variantUploadResult?.Error?.Message ?? "Không nhận được URL ảnh."}");
                }
                var variant = _mapper.Map<ProductVariant>(variantDto);
                variant.Id = Guid.NewGuid().ToString(); 
                variant.ProductId = product.Id; 
                variant.ImageUrl = variantUploadResult.SecureUrl.ToString(); 

                finalVariants.Add(variant); 
            }

            product.Variants = finalVariants; 

            Console.WriteLine($"Product.ImageUrl TRƯỚC khi AddAsync: '{product.ImageUrl}'");
            Console.WriteLine($"Số lượng Variants TRƯỚC khi AddAsync: {product.Variants?.Count ?? 0}");
            if (product.Variants != null)
            {
                foreach (var variant in product.Variants)
                {
                    Console.WriteLine($"  - Final Variant: Color={variant.Color}, Size={variant.Size}, ImageUrl='{variant.ImageUrl}'");
                }
            }
            Console.WriteLine($"--- Kết thúc Debug ---");

            await _productRepository.AddAsync(product);

            var createdProduct = await _productRepository.GetByIdAsync(product.Id);
            if (createdProduct == null)
            {
                return StatusCode(500, "Không thể lấy sản phẩm vừa tạo.");
            }

            var productDto = _mapper.Map<ProductDto>(createdProduct);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(string id, [FromBody] UpdateProductDto updateProductDto)
        {
            var productToUpdate = await _productRepository.GetByIdAsync(id);
            if (productToUpdate == null)
            {
                return NotFound();
            }

            if (!await _categoryRepository.ExistsAsync(updateProductDto.CategoryId))
            {
                return BadRequest($"Category with ID {updateProductDto.CategoryId} does not exist.");
            }

            _mapper.Map(updateProductDto, productToUpdate);
            productToUpdate.Id = id; 
            await _productRepository.UpdateAsync(productToUpdate);
            return NoContent();
        }

        // PUT api/products/{id}/image (API riêng để cập nhật ảnh sản phẩm chính)
        [HttpPut("{id}/image")]
        public async Task<IActionResult> UpdateProductImage(string id, IFormFile imageFile)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("Image file is required.");
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
                return StatusCode(500, $"Failed to upload new image: {uploadResult.Error.Message}");
            }
            product.ImageUrl = uploadResult.SecureUrl.ToString();

            await _productRepository.UpdateAsync(product);

            return NoContent();
        }

        // DELETE api/products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var product = await _productRepository.GetByIdAsync(id); // Đảm bảo nó include Variants
            if (product == null)
            {
                return NotFound();
            }

            // Xóa ảnh của SẢN PHẨM CHÍNH (nếu có)
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

            // Xóa ảnh của TẤT CẢ CÁC VARIANT
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

            await _productRepository.DeleteAsync(id);
            return NoContent();
        }

        // CÁC API RIÊNG ĐỂ QUẢN LÝ BIẾN THỂ (PRODUCT VARIANT)
        // GET api/products/{productId}/variants
        [HttpGet("{productId}/variants")]
        public async Task<ActionResult<IEnumerable<ProductVariantDto>>> GetProductVariants(string productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound($"Product with ID {productId} not found.");
            }
            var variantDtos = _mapper.Map<IEnumerable<ProductVariantDto>>(product.Variants);
            return Ok(variantDtos);
        }

        // GET api/products/{productId}/variants/{variantId}
        [HttpGet("{productId}/variants/{variantId}")]
        public async Task<ActionResult<ProductVariantDto>> GetProductVariant(string productId, string variantId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound($"Product with ID {productId} not found.");
            }
            var variant = product.Variants.FirstOrDefault(v => v.Id == variantId);
            if (variant == null)
            {
                return NotFound($"Product variant with ID {variantId} not found for product {productId}.");
            }
            var variantDto = _mapper.Map<ProductVariantDto>(variant);
            return Ok(variantDto);
        }


        // POST api/products/{productId}/variants

        [HttpPost("{productId}/variants")]
        public async Task<ActionResult<ProductVariantDto>> AddProductVariant(string productId, [FromBody] CreateProductVariantDto createVariantDto)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound($"Product with ID {productId} not found.");
            }



            var newVariant = _mapper.Map<ProductVariant>(createVariantDto);
            newVariant.Id = Guid.NewGuid().ToString();
            newVariant.ProductId = productId; // Gán khóa ngoại




            product.Variants.Add(newVariant); // Thêm vào collection của Product

            await _productRepository.UpdateAsync(product); // Cập nhật Product để lưu Variant mới

            var variantDto = _mapper.Map<ProductVariantDto>(newVariant);
            return CreatedAtAction(nameof(GetProductVariant), new { productId = productId, variantId = newVariant.Id }, variantDto);
        }

        // PUT api/products/{productId}/variants/{variantId}
        [HttpPut("{productId}/variants/{variantId}")]
        public async Task<IActionResult> UpdateProductVariant(string productId, string variantId, [FromBody] UpdateProductVariantDto updateVariantDto)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound($"Product with ID {productId} not found.");
            }

            var existingVariant = product.Variants.FirstOrDefault(v => v.Id == variantId);
            if (existingVariant == null)
            {
                return NotFound($"Product variant with ID {variantId} not found for product {productId}.");
            }

            _mapper.Map(updateVariantDto, existingVariant); 
            existingVariant.Id = variantId; 
            existingVariant.ProductId = productId; 



            await _productRepository.UpdateAsync(product); 

            return NoContent();
        }

        // DELETE api/products/{productId}/variants/{variantId}
        [HttpDelete("{productId}/variants/{variantId}")]
        public async Task<IActionResult> DeleteProductVariant(string productId, string variantId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound($"Product with ID {productId} not found.");
            }

            var variantToRemove = product.Variants.FirstOrDefault(v => v.Id == variantId);
            if (variantToRemove == null)
            {
                return NotFound($"Product variant with ID {variantId} not found for product {productId}.");
            }

            // Xóa ảnh của variant trước khi xóa variant khỏi DB
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

            return NoContent();
        }
    }
}