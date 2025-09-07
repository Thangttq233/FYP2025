using FYP2025.Application.DTOs;
using FYP2025.Application.Services.ProductService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace FYP2025.Api.Features.Products
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var productDtos = await _productService.GetProductsAsync();
            return Ok(productDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(string id)
        {
            var productDto = await _productService.GetProductByIdAsync(id);
            if (productDto == null)
            {
                return NotFound();
            }
            return Ok(productDto);
        }

        [HttpGet("byCategory/{categoryId}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategoryId(string categoryId)
        {
            try
            {
                var productDtos = await _productService.GetProductsByCategoryIdAsync(categoryId);
                return Ok(productDtos);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromForm] CreateProductDto createProductDto)
        {
            try
            {
                var productDto = await _productService.CreateProductAsync(createProductDto);
                return CreatedAtAction(nameof(GetProduct), new { id = productDto.Id }, productDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProductDto>> UpdateProduct(string id, [FromForm] UpdateProductDto updateProductDto)
        {
            try
            {
                var productDto = await _productService.UpdateProductAsync(id, updateProductDto);
                if (productDto == null)
                {
                    return NotFound();
                }
                return Ok(productDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/image")]
        public async Task<IActionResult> UpdateProductImage(string id, IFormFile imageFile)
        {
            try
            {
                await _productService.UpdateProductImageAsync(id, imageFile);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{productId}/variants")]
        public async Task<ActionResult<IEnumerable<ProductVariantDto>>> GetProductVariants(string productId)
        {
            try
            {
                var variantDtos = await _productService.GetProductVariantsAsync(productId);
                return Ok(variantDtos);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{productId}/variants/{variantId}")]
        public async Task<ActionResult<ProductVariantDto>> GetProductVariant(string productId, string variantId)
        {
            try
            {
                var variantDto = await _productService.GetProductVariantAsync(productId, variantId);
                return Ok(variantDto);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{productId}/variants")]
        public async Task<ActionResult<ProductVariantDto>> AddProductVariant(string productId, [FromBody] CreateProductVariantDto createVariantDto)
        {
            try
            {
                var variantDto = await _productService.AddProductVariantAsync(productId, createVariantDto);
                return CreatedAtAction(nameof(GetProductVariant), new { productId = productId, variantId = variantDto.Id }, variantDto);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut("{productId}/variants/{variantId}")]
        public async Task<IActionResult> UpdateProductVariant(string productId, string variantId, [FromForm] UpdateProductVariantDto updateVariantDto)
        {
            try
            {
                await _productService.UpdateProductVariantAsync(productId, variantId, updateVariantDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{productId}/variants/{variantId}")]
        public async Task<IActionResult> DeleteProductVariant(string productId, string variantId)
        {
            try
            {
                await _productService.DeleteProductVariantAsync(productId, variantId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}