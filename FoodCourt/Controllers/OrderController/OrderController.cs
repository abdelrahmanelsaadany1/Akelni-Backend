using Domain.Dtos.OrderDto;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Services.Abstractions.ICategoryService;
using Stripe.Checkout;
using Stripe;
using System.Security.Claims;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // Require authentication for all order operations
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly StripeSettings _stripeSettings;
        

        public OrdersController(IOptions<StripeSettings> stripeSettings, IOrderService orderService)
        {
            _orderService = orderService;
            _stripeSettings = stripeSettings.Value;
        }

        //[Authorize("Roles = Admin")]
        //[HttpPost("create-order")]
        //public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDto dto)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    try
        //    {
        //        var order = new Order
        //        {
        //            RestaurantId = dto.RestaurantId,
        //            DistanceKm = dto.DistanceKm,
        //            CreatedAt = DateTime.UtcNow,
        //            Status = Order.OrderStatus.Pending
        //            // CustomerId will be set automatically in the service from claims
        //        };

        //        // after order validation, redirect to the payment page then add order to chef dashboard.
        //        //this.CreateCheckoutSession(amount);

        //        await _orderService.AddOrderAsync(order, dto.Items);
        //        return Ok(new { message = "Order created successfully", orderId = order.Id });
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        return Unauthorized(ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}

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
        public async  Task<IActionResult> CreateCheckoutSession([FromBody] OrderCreateDto dto)
        {
            int OrderId;

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
                

                await _orderService.AddOrderAsync(order, dto.Items, Convert.ToInt32(dto.amount));
                //return Ok(new { message = "Order created successfully", orderId = order.Id });
                OrderId = order.Id;
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

            int amount = Convert.ToInt32(dto.amount); 

            //var domain = "http://localhost:4200";
            var domain = "https://localhost:7045";
            var currency = "egp";
            var successUrl = domain + "/api/Orders/success";
            var cancelUrl = domain + "/api/Orders/cancel";
            //var successUrl = domain + "/success";
            //var cancelUrl = domain + "/cancel";
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = currency,
                        UnitAmount = Convert.ToInt32(amount) * 100,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Total Fees",
                        }
                    },
                   Quantity = 1
                },
            },
                Metadata = new Dictionary<string, string> {
                    { "OrderId",  OrderId.ToString() }
                },
                Mode = "payment",
                SuccessUrl = successUrl + "?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = cancelUrl,
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Ok(new { url = session.Url, id = session.Id, amount });
        }

        [HttpGet("success")]
        public Task<IActionResult>  Success([FromQuery] string session_id)
        {
            //if (string.IsNullOrEmpty(session_id))
            //    return BadRequest("No session ID provided");

            //SessionService service = new SessionService();
            //var session = await service.GetAsync(session_id);

            //int OrderId = int.Parse(session.Metadata["OrderId"]);

            //if(OrderId <= 0)
            //{
            //    return BadRequest();
            //}

            //try
            //{
            //    //since a successful payment is placed into stripe dashboard, change the status to paid
            //    await _orderService.UpdateOrderStatusAsync(OrderId, Order.OrderStatus.Paid);
            //    return Ok();

            //} catch (Exception e)
            //{
            //    return NotFound(new { msg = e.Message });
            //}

            return CheckoutStatus(session_id, Order.OrderStatus.Paid);

        }

        [HttpGet, Route("cancel")]
        public Task<IActionResult> Cancel([FromQuery] string session_id)
        {
            return CheckoutStatus(session_id, Order.OrderStatus.Cancelled);
        }

        private async Task<IActionResult> CheckoutStatus(string session_id, Order.OrderStatus status)
        {
            if (string.IsNullOrEmpty(session_id))
                return BadRequest("No session ID provided");

            SessionService service = new SessionService();
            var session = await service.GetAsync(session_id);

            int OrderId = int.Parse(session.Metadata["OrderId"]);

            if (OrderId <= 0)
            {
                return BadRequest();
            }

            try
            {
                //since a successful payment is placed into stripe dashboard, change the status to paid
                //either is cancelled set it to cancelled
                await _orderService.UpdateOrderStatusAsync(OrderId, status);
                return Ok(new {status = ((Order.OrderStatus) status).ToString(), url = "http://localhost:4200" });
            }
            catch (Exception e)
            {
                return NotFound(new { msg = e.Message });
            }
        }
    }
}