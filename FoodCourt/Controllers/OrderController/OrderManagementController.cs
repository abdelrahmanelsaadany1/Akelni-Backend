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
                await _notificationService.NotifyOrderStatusToChef(restaurant.ChefId, orderId, "accepted");

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
                await _notificationService.NotifyOrderStatusToChef(restaurant.ChefId, orderId, "rejected");

                return Ok(new { message = "Order rejected successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // For Chef Dashboard order component to view all details of the order
        // C#
        [HttpGet("chef/current")]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> GetChefCurrentOrders()
        {
            var chefId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Get only active orders (Pending/Accepted/InProgress/etc.)
            var orders = await _orderService.GetCurrentOrdersForChefAsync(chefId);
            return Ok(orders);
        }


        [HttpGet("orders/{orderId}")]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order is null) return NotFound();

            // Validate chef owns the restaurant
            var chefId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var restaurant = await _restaurantService.GetRestaurantByIdAsync(order.RestaurantId);
            if (restaurant.ChefId != chefId) return Forbid();

            // Return full details (items, delivery address, totals, customer info, etc.)
            var details = await _orderService.GetOrderDetailsAsync(orderId);
            return Ok(details);
        }

    }
    public class RejectOrderDto
    {
        public string Reason { get; set; } = "Restaurant is busy";
    }
}
