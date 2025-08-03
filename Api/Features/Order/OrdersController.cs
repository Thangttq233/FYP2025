using FYP2025.Application.DTOs;
using FYP2025.Application.Services.OrderService;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Collections.Generic;
using FYP2025.Application.Common; 

namespace FYP2025.Api.Features.Order 
{
    [ApiController]
    [Route("api/orders")]
    [Authorize] 
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // Lấy UserId từ JWT token
        private string GetUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }
            return userId;
        }

        // GET: api/orders/my-orders
        [HttpGet("my-orders")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetUserOrders()
        {
            try
            {
                var userId = GetUserId();
                var orders = await _orderService.GetUserOrdersAsync(userId);
                return Ok(orders);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // GET: api/orders/{orderId}
        [HttpGet("{orderId}")]
        public async Task<ActionResult<OrderDto>> GetOrderDetails(string orderId)
        {
            try
            {
                var order = await _orderService.GetOrderDetailsAsync(orderId);
                if (order == null)
                {
                    return NotFound($"Order with ID {orderId} not found.");
                }

                // Đảm bảo chỉ user sở hữu order hoặc Admin/Saler mới được xem
                if (order.UserId != GetUserId() && !User.IsInRole(nameof(RolesEnum.Admin)) && !User.IsInRole(nameof(RolesEnum.Saler)))
                {
                    return Forbid(); // Trả về 403 Forbidden nếu không có quyền
                }

                return Ok(order);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // POST: api/orders/create
        [HttpPost("create")]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetUserId();
                var order = await _orderService.CreateOrderFromCartAsync(userId, request);
                return CreatedAtAction(nameof(GetOrderDetails), new { orderId = order.Id }, order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
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

        // PUT: api/orders/update-status (Chỉ Admin hoặc Saler)
        [HttpPut("update-status")]
        [Authorize(Roles = $"{nameof(RolesEnum.Admin)},{nameof(RolesEnum.Saler)}")] 
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _orderService.UpdateOrderStatusAsync(request.OrderId, request);
            if (result)
            {
                return NoContent();
            }
            return NotFound($"Order with ID {request.OrderId} not found.");
        }
    }
}