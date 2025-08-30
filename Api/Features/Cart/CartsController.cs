using FYP2025.Application.DTOs;
using FYP2025.Application.Services.CartService; 
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; 
using System.Security.Claims; 

namespace FYP2025.Api.Features.Cart 
{
    [ApiController]
    [Route("api/carts")]
    [Authorize] 
    public class CartsController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartsController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // Lấy UserId từ JWT token
        private string GetUserId()
        {
            var userId = (User as ClaimsPrincipal)?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }
            return userId;
        }

        // GET: api/carts/my-cart
        [HttpGet("my-cart")]
        public async Task<ActionResult<CartDto>> GetUserCart()
        {
            try
            {
                var userId = GetUserId();
                var cart = await _cartService.GetUserCartAsync(userId);
                return Ok(cart);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // POST: api/carts/add-item
        [HttpPost("add-item")]
        public async Task<ActionResult<CartDto>> AddItemToCart([FromBody] AddToCartRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetUserId();
                var cart = await _cartService.AddItemToCartAsync(userId, request);
                return Ok(cart);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // PUT: api/carts/update-item
        [HttpPut("update-item")]
        public async Task<ActionResult<CartDto>> UpdateCartItemQuantity([FromBody] UpdateCartItemRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetUserId();
                var cart = await _cartService.UpdateCartItemQuantityAsync(userId, request);
                return Ok(cart);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // DELETE: api/carts/remove-item/{cartItemId}
        [HttpDelete("remove-item/{cartItemId}")]
        public async Task<ActionResult<CartDto>> RemoveCartItem(string cartItemId)
        {
            try
            {
                var userId = GetUserId();
                var cart = await _cartService.RemoveCartItemAsync(userId, cartItemId);
                return Ok(cart);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}