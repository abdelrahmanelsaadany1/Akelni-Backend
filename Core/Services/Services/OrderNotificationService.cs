using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Dtos.OrderDto;
using Domain.Entities;
using Domain.Hubs;
using Microsoft.AspNetCore.SignalR;
using Services.Abstractions.IServices;

namespace Services.Services
{
    public class OrderNotificationService : IOrderNotificationService
    {
        private readonly IHubContext<OrderHub> _hubContext;
        public OrderNotificationService(IHubContext<OrderHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public async Task SendOrderRequestToChef(string chefId, OrderResponseDto order)
        {
            await _hubContext.Clients.Group($"Chef_{chefId}").SendAsync("ReceiveOrderRequest", new
            {
                OrderId = order.Id,
                CustomerName = "Customer",
                Items = order.Items,
                TotalAmount = order.SubTotal,
                CreatedAt = order.CreatedAt,
                RestaurantName = order.RestaurantName
            });
        }
        public async Task NotifyCustomerOrderAccepted(string customerId, int orderId)
        {
            await _hubContext.Clients.Group($"Customer_{customerId}").SendAsync("OrderAccepted", new
            {
                OrderId = orderId,
                Message = "Your order has been accepted! Redirecting to payment...",
                TimeStamp = DateTime.UtcNow
            });
        }
        public async Task NotifyCustomerOrderRejected(string customerId, int orderId, string reason)
        {
            await _hubContext.Clients.Group($"Customer_{customerId}").SendAsync("OrderRejected", new
            {
                OrderId = orderId,
                Message = $"Your order has been rejected. Reason: {reason}",
                Timestamp = DateTime.UtcNow
            });
        }
        public async Task NotifyChefOrderPaid(string chefId, int orderId)
        {
            await _hubContext.Clients.Group($"Chef_{chefId}").SendAsync("OrderPaid", new
            {
                OrderId = orderId,
                Message = "Customer has completed payment. You can start preparing the order.",
                Timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyChefOrderCancelled(string chefId, int orderId)
        {
            await _hubContext.Clients.Group($"Chef_{chefId}").SendAsync("OrderPaymentCancelled", new
            {
                OrderId = orderId,
                Message = "Customer cancelled the payment. Do not prepare this order.",
                Timestamp = DateTime.UtcNow
            });
        }
        public async Task NotifyCustomerOrderStatusUpdate(string customerId, int orderId, Order.OrderStatus status)
        {
            await _hubContext.Clients.Group($"Customer_{customerId}").SendAsync("OrderStatusUpdate", new
            {
                OrderId = orderId,
                Status = status.ToString(),
                Message = GetStatusMessage(status),
                Timestamp = DateTime.UtcNow
            });
        }

        private string GetStatusMessage(Order.OrderStatus status)
        {
            return status switch
            {
                Order.OrderStatus.Accepted => "Your order has been accepted and is being prepared.",
                Order.OrderStatus.InTransit => "Your order is on the way!",
                Order.OrderStatus.Delivered => "Your order has been delivered. Enjoy!",
                Order.OrderStatus.Cancelled => "Your order has been cancelled.",
                _ => $"Order status updated to {status}"
            };
        }
    }
}
