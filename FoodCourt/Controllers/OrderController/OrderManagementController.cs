using System.Security.Claims;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions.ICategoryService;
using Services.Abstractions.IServices;


namespace FoodCourt.Controllers.OrderController
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderManagementController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IOrderNotificationService _notificationService;
        private readonly IResturantService _restaurantService;

        public OrderManagementController(IOrderService orderService, IOrderNotificationService notificationService, IResturantService resturantService)
        {
            _orderService = orderService;
            _notificationService = notificationService;
            _restaurantService = resturantService;
        }

        [HttpPost("{orderId}/accept")]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> AcceptOrder(int orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    return NotFound();
                }
                var currentChefId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var restaurant = await _restaurantService.GetRestaurantByIdAsync(order.RestaurantId);

                if (restaurant.ChefId != currentChefId)
                {
                    return Forbid("You can only accept orders for your own restaurant.");
                }
                if (order.Status != Order.OrderStatus.Pending)
                {
                    return BadRequest("Only pending orders can be accepted.");
                }
                await _orderService.UpdateOrderStatusAsync(orderId, Order.OrderStatus.Accepted);
                await _notificationService.NotifyCustomerOrderAccepted(order.CustomerId, orderId);

                return Ok(new { message = "Order accepted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{orderId}/reject")]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> RejectOrder(int orderId, [FromBody] RejectOrderDto dto)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);

                // Verify chef owns this restaurant
                var currentChefId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var restaurant = await _restaurantService.GetRestaurantByIdAsync(order.RestaurantId);

                if (restaurant.ChefId != currentChefId)
                {
                    return Forbid("You can only reject orders for your own restaurant.");
                }

                if (order.Status != Order.OrderStatus.Pending)
                {
                    return BadRequest("Only pending orders can be rejected.");
                }

                await _orderService.UpdateOrderStatusAsync(orderId, Order.OrderStatus.Rejected);
                await _notificationService.NotifyCustomerOrderRejected(order.CustomerId, orderId, dto.Reason);

                return Ok(new { message = "Order rejected successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
    public class RejectOrderDto
    {
        public string Reason { get; set; } = "Restaurant is busy";
    }
}
