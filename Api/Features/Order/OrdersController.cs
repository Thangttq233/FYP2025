using FYP2025.Application.DTOs;
using FYP2025.Application.Services.OrderService;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Collections.Generic;
using FYP2025.Application.Common;
using FYP2025.Application.Services.Vnpay;
namespace FYP2025.Api.Features.Order
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IVnpayService _vnpayService;
        public OrdersController(IOrderService orderService, IVnpayService vnpayService)
        {
            _orderService = orderService;
            _vnpayService = vnpayService;
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

        private string GetIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "::1";
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

        // POST: api/orders/{orderId}/pay
        [HttpPost("pay")]
        public async Task<IActionResult> CreateVnpayPayment([FromBody] OrderDto order)
        {
            if (order == null)
            {
                return BadRequest("Order cannot be null");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid order data");
            }

            var paymentUrl = await _vnpayService.CreateVnpayPaymentUrl(order);
            return Ok(new { paymentUrl });
        }
        [HttpPost("returnURL")]
        public async Task<IActionResult> VnpayReturn([FromBody] ReturnDto request)
        {
            if (string.IsNullOrEmpty(request.ResponseCode) || string.IsNullOrEmpty(request.OrderId))
            {
                return BadRequest(new { message = "Payment processing failed" });
            }

            await _vnpayService.HandleVnpayUrl(request.ResponseCode, request.OrderId);
            return Ok(new { message = "Payment processed successfully" });
        }


    }
}