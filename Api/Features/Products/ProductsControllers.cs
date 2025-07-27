using FYP2025.Application.DTOs;
using FYP2025.Domain.Entities;
using FYP2025.Domain.Repositories;
using FYP2025.Domain.Services.Cloudinary;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; // Cần thiết cho IFormFile
using System.IO;
using System;
using System.Linq; // Để sử dụng .Any() và .All()
// using System.Text.Json; // Không cần nếu không dùng VariantsJson

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


        // POST api/products - API chính để tạo sản phẩm
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
            // --- Kết thúc Debugging ---

            if (!await _categoryRepository.ExistsAsync(createProductDto.CategoryId))
            {
                return BadRequest($"Danh mục với ID {createProductDto.CategoryId} không tồn tại.");
            }

            // --- Kiểm tra và upload ảnh cho SẢN PHẨM CHÍNH ---
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

            // --- Kiểm tra Variants ---
            if (createProductDto.Variants == null || !createProductDto.Variants.Any())
            {
                return BadRequest("Cần ít nhất một biến thể sản phẩm.");
            }

            // --- Ánh xạ CreateProductDto sang Product entity ---
            // AutoMapper sẽ map các thuộc tính cơ bản của Product (Name, Description, CategoryId)
            var product = _mapper.Map<Product>(createProductDto);
            product.Id = Guid.NewGuid().ToString();
            product.ImageUrl = productUploadResult.SecureUrl.ToString(); // Gán URL ảnh của SẢN PHẨM CHÍNH

            // --- Xử lý các Variants và ảnh của chúng ---
            // Tạo một danh sách Variants mới để thêm vào Product sau khi xử lý ảnh và ID
            List<ProductVariant> finalVariants = new List<ProductVariant>();

            foreach (var variantDto in createProductDto.Variants) // Lặp qua DTO gốc để lấy ImageFile
            {
                // Kiểm tra file ảnh cho từng variant
                if (variantDto.ImageFile == null || variantDto.ImageFile.Length == 0)
                {
                    return BadRequest($"Biến thể '{variantDto.Color} - {variantDto.Size}' yêu cầu file ảnh.");
                }

                // Upload ảnh của từng variant
                var variantUploadResult = await _photoService.UploadPhotoAsync(variantDto.ImageFile);
                if (variantUploadResult.Error != null || variantUploadResult.SecureUrl == null)
                {
                    Console.WriteLine($"Tải ảnh lên Cloudinary thất bại cho biến thể '{variantDto.Color} - {variantDto.Size}': {variantUploadResult?.Error?.Message ?? "URL an toàn là null/rỗng"}");
                    return StatusCode(500, $"Tải ảnh lên thất bại cho biến thể '{variantDto.Color} - {variantDto.Size}': {variantUploadResult?.Error?.Message ?? "Không nhận được URL ảnh."}");
                }

                // Ánh xạ CreateProductVariantDto sang ProductVariant entity
                var variant = _mapper.Map<ProductVariant>(variantDto);
                variant.Id = Guid.NewGuid().ToString(); // Tạo ID cho variant
                variant.ProductId = product.Id; // Gán khóa ngoại
                variant.ImageUrl = variantUploadResult.SecureUrl.ToString(); // Gán URL ảnh đã upload cho variant

                finalVariants.Add(variant); // Thêm variant đã hoàn chỉnh vào danh sách tạm
            }

            product.Variants = finalVariants; // Gán danh sách variants đã hoàn chỉnh vào sản phẩm

            // --- Debugging: Kiểm tra giá trị cuối cùng trước khi lưu ---
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
            // --- Kết thúc Debugging ---

            await _productRepository.AddAsync(product);

            // Lấy lại sản phẩm từ DB để đảm bảo tất cả các quan hệ (Category, Variants) được tải đúng cách
            var createdProduct = await _productRepository.GetByIdAsync(product.Id);
            if (createdProduct == null)
            {
                return StatusCode(500, "Không thể lấy sản phẩm vừa tạo.");
            }

            var productDto = _mapper.Map<ProductDto>(createdProduct);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDto);
        }

        // PUT api/products/{id} - Chỉ cập nhật thông tin chính của sản phẩm (không bao gồm Variants)
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

            // Map các thuộc tính chính của Product từ DTO
            _mapper.Map(updateProductDto, productToUpdate);
            productToUpdate.Id = id; // Đảm bảo ID không thay đổi

            // LƯU Ý: Phần cập nhật Variants và ImageUrl của Product chính sẽ KHÔNG được xử lý tự động ở đây.
            // Bạn cần các endpoint riêng cho việc này hoặc logic phức tạp hơn.

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

        // ----------------------------------------------------
        // CÁC API RIÊNG ĐỂ QUẢN LÝ BIẾN THỂ (PRODUCT VARIANT)
        // ----------------------------------------------------

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
        // API này hiện tại đang nhận [FromBody] CreateProductVariantDto
        // Nếu bạn muốn API này cũng cho phép upload ảnh cho variant, bạn sẽ cần thay đổi nó thành [FromForm]
        // và thêm IFormFile vào CreateProductVariantDto.
        // Tuy nhiên, để nhất quán với cách CreateProduct đang làm, chúng ta sẽ giả định
        // AddProductVariant sẽ được gọi riêng hoặc ảnh đã có (qua URL)
        // hoặc bạn sẽ cần một API UpdateProductVariantImage riêng cho variant.
        [HttpPost("{productId}/variants")]
        public async Task<ActionResult<ProductVariantDto>> AddProductVariant(string productId, [FromBody] CreateProductVariantDto createVariantDto)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound($"Product with ID {productId} not found.");
            }

            // Nếu bạn muốn cho phép upload ảnh variant qua API này, bạn phải thay đổi [FromBody] thành [FromForm]
            // và sửa CreateProductVariantDto để có IFormFile.
            // Ví dụ đơn giản:
            // if (createVariantDto.ImageFile == null || createVariantDto.ImageFile.Length == 0) { return BadRequest("Image required for variant."); }
            // var uploadResult = await _photoService.UploadPhotoAsync(createVariantDto.ImageFile);
            // newVariant.ImageUrl = uploadResult.SecureUrl.ToString();
            // Nhưng hiện tại, API này chỉ nhận JSON, không có file.

            var newVariant = _mapper.Map<ProductVariant>(createVariantDto);
            newVariant.Id = Guid.NewGuid().ToString();
            newVariant.ProductId = productId; // Gán khóa ngoại

            // LƯU Ý: Tại đây ImageUrl của newVariant sẽ là NULL/rỗng nếu không được gán thủ công từ file upload
            // hoặc không được truyền trong DTO JSON và DB là NOT NULL.
            // Nếu ImageUrl là NOT NULL trong DB, bạn cần đảm bảo nó có giá trị ở đây.
            // Giải pháp: Sử dụng API UpdateProductVariantImage sau khi tạo variant,
            // hoặc thay đổi API này để nhận [FromForm] và file.


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

            _mapper.Map(updateVariantDto, existingVariant); // Cập nhật thuộc tính của biến thể
            existingVariant.Id = variantId; // Đảm bảo ID không đổi
            existingVariant.ProductId = productId; // Đảm bảo khóa ngoại không đổi

            // LƯU Ý: ImageUrl của variant sẽ không được cập nhật qua API này nếu nó chỉ nhận [FromBody]
            // và UpdateProductVariantDto không có IFormFile.
            // Bạn cần sử dụng API UpdateProductVariantImage riêng để cập nhật ảnh.

            await _productRepository.UpdateAsync(product); // Cập nhật Product để lưu thay đổi của Variant

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

            product.Variants.Remove(variantToRemove); // Xóa khỏi collection của Product

            await _productRepository.UpdateAsync(product); // Cập nhật Product để lưu thay đổi

            return NoContent();
        }
    }
}