using System.Security.Claims;
using Domain.Dtos.OrderDto;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Services.Abstractions.ICategoryService;
using Services.Abstractions.IServices;
using Stripe;
using Stripe.Checkout;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly StripeSettings _stripeSettings;
        private readonly IOrderNotificationService _notificationService;
        private readonly IResturantService _restaurantService;
        private readonly IConfiguration _configuration;

        public OrdersController(
            IOptions<StripeSettings> stripeSettings,
            IOrderService orderService,
            IOrderNotificationService notificationService,
            IResturantService restaurantService,
            IConfiguration configuration
            )
        {
            _orderService = orderService;
            _stripeSettings = stripeSettings.Value;
            _notificationService = notificationService;
            _restaurantService = restaurantService;
            _configuration = configuration;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")] // Only admins can see all orders
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);

                // Check if user owns this order or is admin/chef
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);

                if (order.CustomerId != currentUserId &&
                    !userRoles.Contains("Admin") &&
                    !userRoles.Contains("Chef"))
                {
                    return Forbid("You don't have permission to view this order.");
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [HttpGet("my-orders")]
        [Authorize(Roles = "Customer")]  // Only customers can access their orders
        public async Task<IActionResult> GetMyOrders()
        {
            try
            {
                var orders = await _orderService.GetCurrentUserOrdersAsync();
                return Ok(orders);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("customer/{customerId}")]
        [Authorize(Roles = "Admin")] // Only admins can search by customer ID
        public async Task<IActionResult> GetOrdersByCustomerId(string customerId)
        {
            try
            {
                var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("restaurant/{restaurantId}")]
        [Authorize(Roles = "Chef,Admin")] // Chefs can see orders for their restaurants
        public async Task<IActionResult> GetOrdersByRestaurantId(int restaurantId)
        {
            try
            {
                var orders = await _orderService.GetOrdersByRestaurantIdAsync(restaurantId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("status/{status}")]
        [Authorize(Roles = "Chef,Admin")] // Chefs and admins can filter by status
        public async Task<IActionResult> GetOrdersByStatus(Order.OrderStatus status)
        {
            try
            {
                var orders = await _orderService.GetOrdersByStatusAsync(status);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Chef,Admin")] // Only chefs and admins can update order status
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatusUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _orderService.UpdateOrderStatusAsync(id, dto.Status);
                return Ok(new { message = "Order status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only admins can delete orders
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                await _orderService.DeleteOrderAsync(id);
                return Ok(new { message = "Order deleted successfully" });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/total")]
        public async Task<IActionResult> GetOrderTotal(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);

                if (order.CustomerId != currentUserId &&
                    !userRoles.Contains("Admin") &&
                    !userRoles.Contains("Chef"))
                {
                    return Forbid("You don't have permission to view this order.");
                }

                var total = await _orderService.CalculateOrderTotalAsync(id);
                return Ok(new { orderId = id, totalAmount = total });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("create-ckeckout-session")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] OrderCreateDto dto)
        {
            int OrderId;
            decimal OrderTotal;

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var order = new Order
                {
                    RestaurantId = dto.RestaurantId,
                    DistanceKm = dto.DistanceKm,
                    CreatedAt = DateTime.UtcNow,
                    Status = Order.OrderStatus.Pending
                    // CustomerId will be set automatically in the service from claims
                };

                await _orderService.AddOrderAsync(order, dto.Items);
                OrderId = order.Id;

                // ✅ Calculate total including add-ons and combos using the service method
                OrderTotal = await _orderService.CalculateOrderTotalAsync(OrderId);

                // Get the order details and restaurant info
                var orderDetails = await _orderService.GetOrderByIdAsync(OrderId);
                var restaurant = await _restaurantService.GetRestaurantByIdAsync(order.RestaurantId);

                // Send notification to chef for approval
                await _notificationService.SendOrderRequestToChef(restaurant.ChefId, orderDetails);

                // Return order details instead of immediately creating Stripe session
                return Ok(new
                {
                    message = "Order created successfully. Waiting for chef approval.",
                    orderId = OrderId,
                    amount = OrderTotal,
                    status = "pending_approval"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // New endpoint for creating Stripe session after chef approval
        [HttpPost("{orderId}/create-payment-session")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreatePaymentSession(int orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);

                // Verify order belongs to current user
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (order.CustomerId != currentUserId)
                {
                    return Forbid("You can only create payment sessions for your own orders.");
                }

                // Verify order is accepted
                if (order.Status != Order.OrderStatus.Accepted)
                {
                    return BadRequest("Order must be accepted by chef before payment.");
                }

                // ✅ Calculate total including add-ons and combos
                decimal amount = await _orderService.CalculateOrderTotalAsync(orderId);

                // ✅ Get environment-appropriate domain URLs
                var domain = GetBaseUrl();
                var currency = "egp";
                var successUrl = domain + "/api/Orders/success";
                var cancelUrl = domain + "/api/Orders/cancel";

                StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

                // ✅ Create detailed line items including add-ons and combos
                var lineItems = await CreateStripeLineItemsAsync(order);

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = lineItems,
                    Metadata = new Dictionary<string, string> {
                        { "OrderId", orderId.ToString() }
                    },
                    Mode = "payment",
                    SuccessUrl = successUrl + "?session_id={CHECKOUT_SESSION_ID}",
                    CancelUrl = cancelUrl + "?session_id={CHECKOUT_SESSION_ID}",
                };

                var service = new SessionService();
                var session = service.Create(options);

                return Ok(new { url = session.Url, id = session.Id, amount });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("success")]
        public async Task<IActionResult> Success([FromQuery] string session_id)
        {
            return await CheckoutStatus(session_id, Order.OrderStatus.Paid);
        }

        [HttpGet("cancel")] 
        public async Task<IActionResult> Cancel([FromQuery] string session_id)
        {
            return await CheckoutStatus(session_id, Order.OrderStatus.Cancelled);
        }

        private async Task<IActionResult> CheckoutStatus(string session_id, Order.OrderStatus status)
        {
            if (string.IsNullOrEmpty(session_id))
                return BadRequest("No session ID provided");

            var service = new SessionService();
            var session = await service.GetAsync(session_id);

            if (!session.Metadata.TryGetValue("OrderId", out var orderIdStr) || !int.TryParse(orderIdStr, out int orderId) || orderId <= 0)
                return BadRequest("Invalid Order ID");

            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                var restaurant = await _restaurantService.GetRestaurantByIdAsync(order.RestaurantId);

                // Update order status
                await _orderService.UpdateOrderStatusAsync(orderId, status);

                if (status == Order.OrderStatus.Paid)
                {
                    // ✅ Record payment with correct total amount including add-ons and combos
                    var totalAmount = await _orderService.CalculateOrderTotalAsync(orderId);

                    await _orderService.CreatePaymentAsync(orderId, new Payment
                    {
                        StripePaymentIntentId = session.PaymentIntentId ?? session.Id,
                        Amount = totalAmount,
                        PaidAt = DateTime.UtcNow,
                        OrderId = orderId
                    });

                    // Notify chef
                    await _notificationService.NotifyChefOrderPaid(restaurant.ChefId, orderId);

                    // ✅ Get environment-appropriate frontend URL
                    var frontendUrl = GetFrontendUrl();
                    return Redirect($"{frontendUrl}/customer/success");
                }
                else
                {
                    // Notify chef
                    await _notificationService.NotifyChefOrderCancelled(restaurant.ChefId, orderId);

                    // ✅ Get environment-appropriate frontend URL
                    var frontendUrl = GetFrontendUrl();
                    return Redirect($"{frontendUrl}/customer/error");
                }
            }
            catch (Exception e)
            {
                return NotFound(new { msg = e.Message });
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// ✅ Gets the appropriate base URL based on environment
        /// </summary>
        private string GetBaseUrl()
        {
            // Check if running in development
            if (_configuration["ASPNETCORE_ENVIRONMENT"] == "Development")
            {
                return "https://localhost:7045";
            }

            // For production, try to get from configuration or request
            var apiUrl = _configuration["App:ApiUrl"];
            if (!string.IsNullOrEmpty(apiUrl))
            {
                return apiUrl.TrimEnd('/');
            }

            // Fallback to request URL
            var request = HttpContext.Request;
            return $"{request.Scheme}://{request.Host}";
        }

        /// <summary>
        /// ✅ Gets the appropriate frontend URL based on environment
        /// </summary>
        private string GetFrontendUrl()
        {
            var frontendUrl = _configuration["App:FrontendUrl"];
            if (!string.IsNullOrEmpty(frontendUrl))
            {
                return frontendUrl.TrimEnd('/');
            }

            // Fallback for development
            return "http://localhost:4200";
        }

        /// <summary>
        /// ✅ Creates detailed Stripe line items including add-ons and combos
        /// </summary>
        private async Task<List<SessionLineItemOptions>> CreateStripeLineItemsAsync(Domain.Dtos.OrderDto.OrderResponseDto order)
        {
            var lineItems = new List<SessionLineItemOptions>();

            foreach (var item in order.Items)
            {
                // Main item
                var itemTotal = item.ItemPrice * item.Quantity;

                // Add add-ons to item total
                var addOnTotal = item.AddOns?.Sum(a => a.AddOnPrice * item.Quantity) ?? 0;

                // Add combos to item total
                var comboTotal = item.Combos?.Sum(c => c.ComboPrice * item.Quantity) ?? 0;

                var finalItemTotal = itemTotal + addOnTotal + comboTotal;

                var itemDescription = item.ItemName;
                if (item.AddOns?.Any() == true)
                {
                    var addOnNames = string.Join(", ", item.AddOns.Select(a => a.AddOnName));
                    itemDescription += $" with add-ons: {addOnNames}";
                }
                if (item.Combos?.Any() == true)
                {
                    var comboNames = string.Join(", ", item.Combos.Select(c => c.ComboName));
                    itemDescription += $" with combos: {comboNames}";
                }

                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "egp",
                        UnitAmount = Convert.ToInt64(finalItemTotal * 100), // Convert to cents
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = itemDescription,
                            Description = $"Item: {item.ItemName}"
                        }
                    },
                    Quantity = item.Quantity
                });
            }

            // Add delivery fee if any
            if (order.DeliveryFee > 0)
            {
                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "egp",
                        UnitAmount = Convert.ToInt64(order.DeliveryFee * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Delivery Fee"
                        }
                    },
                    Quantity = 1
                });
            }

            // Add platform fee if any
            if (order.PlatformFee > 0)
            {
                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "egp",
                        UnitAmount = Convert.ToInt64(order.PlatformFee * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Platform Fee"
                        }
                    },
                    Quantity = 1
                });
            }

            return lineItems;
        }

        #endregion
    }
}