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
            string customerName = "Customer";
            await _hubContext.Clients.Group($"Chef_{chefId}").SendAsync("ReceiveOrderRequest", new
            {
                orderId = order.Id,
                customerName = order.CustomerName ?? "Customer",
                items = order.Items,
                totalAmount = order.SubTotal,
                createdAt = order.CreatedAt,
                restaurantName = order.RestaurantName
            });
        }

        public async Task NotifyCustomerOrderAccepted(string customerId, int orderId)
        {
            await _hubContext.Clients.Group($"Customer_{customerId}").SendAsync("OrderAccepted", new
            {
                orderId,
                message = "Your order has been accepted! Redirecting to payment...",
                timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyCustomerOrderRejected(string customerId, int orderId, string reason)
        {
            await _hubContext.Clients.Group($"Customer_{customerId}").SendAsync("OrderRejected", new
            {
                orderId,
                message = $"Your order has been rejected. Reason: {reason}",
                timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyChefOrderPaid(string chefId, int orderId)
        {
            await _hubContext.Clients.Group($"Chef_{chefId}").SendAsync("OrderPaid", new
            {
                orderId,
                message = "Customer has completed payment. You can start preparing the order.",
                timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyChefOrderCancelled(string chefId, int orderId)
        {
            await _hubContext.Clients.Group($"Chef_{chefId}").SendAsync("OrderPaymentCancelled", new
            {
                orderId,
                message = "Customer cancelled the payment. Do not prepare this order.",
                timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyCustomerOrderStatusUpdate(string customerId, int orderId, Order.OrderStatus status)
        {
            await _hubContext.Clients.Group($"Customer_{customerId}").SendAsync("OrderStatusUpdate", new
            {
                orderId,
                status = status.ToString().ToLowerInvariant(),
                message = GetStatusMessage(status),
                timestamp = DateTime.UtcNow
            });
        }


        public async Task NotifyChefNewOrder(string chefId, int orderId, object payload)
        {
            await _hubContext.Clients.Group($"Chef_{chefId}")
                .SendAsync("ReceiveOrderRequest", payload);
        }


        public async Task NotifyOrderStatusToChef(string chefId, int orderId, string status)
        {
            var normalized = (status ?? "pending").ToLowerInvariant();
            await _hubContext.Clients.Group($"Chef_{chefId}").SendAsync("OrderStatusUpdate", new
            {
                orderId,
                status = normalized,
                message = GetStatusMessage(ParseStatusSafe(normalized)),
                timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyOrderStatusToCustomer(string customerId, int orderId, string status)
        {
            var normalized = (status ?? "pending").ToLowerInvariant();
            await _hubContext.Clients.Group($"Customer_{customerId}").SendAsync("OrderStatusUpdate", new
            {
                orderId,
                status = normalized,
                message = GetStatusMessage(ParseStatusSafe(normalized)),
                timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyOrderPaid(string customerId, string chefId, int orderId, decimal amount)
        {
            var payload = new
            {
                orderId,
                amount,
                message = "Payment received.",
                timestamp = DateTime.UtcNow
            };
            await Task.WhenAll(
                _hubContext.Clients.Group($"Customer_{customerId}").SendAsync("OrderPaid", payload),
                _hubContext.Clients.Group($"Chef_{chefId}").SendAsync("OrderPaid", payload)
            );
        }

        public async Task NotifyOrderPaymentCancelled(string customerId, string chefId, int orderId, string reason)
        {
            var payload = new
            {
                orderId,
                reason,
                message = "Payment cancelled.",
                timestamp = DateTime.UtcNow
            };
            await Task.WhenAll(
                _hubContext.Clients.Group($"Customer_{customerId}").SendAsync("OrderPaymentCancelled", payload),
                _hubContext.Clients.Group($"Chef_{chefId}").SendAsync("OrderPaymentCancelled", payload)
            );
        }


        private string GetStatusMessage(Order.OrderStatus status)
        {
            return status switch
            {
                Order.OrderStatus.Accepted => "Order accepted and being prepared.",
                Order.OrderStatus.InTransit => "Order is on the way.",
                Order.OrderStatus.Delivered => "Order delivered.",
                Order.OrderStatus.Cancelled => "Order cancelled.",
                Order.OrderStatus.Rejected => "Order rejected.",
                _ => $"Order status updated to {status}"
            };
        }

        private Order.OrderStatus ParseStatusSafe(string statusLower)
        {
            // Map lowercase strings to your enum; extend as needed
            return statusLower switch
            {
                "accepted" => Order.OrderStatus.Accepted,
                "rejected" => Order.OrderStatus.Rejected,
                "cancelled" => Order.OrderStatus.Cancelled,
                "in_transit" => Order.OrderStatus.InTransit,
                "intransit" => Order.OrderStatus.InTransit,
                "delivered" => Order.OrderStatus.Delivered,
                "pending" => Order.OrderStatus.Pending,
                _ => Order.OrderStatus.Pending
            };
        }


        public Task NotifyCustomerOrderDelivered(string customerId, int orderId)
        {
            throw new NotImplementedException();
        }
    }
}
