using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Dtos.OrderDto;
using Domain.Entities;

namespace Services.Abstractions.IServices
{
    public interface IOrderNotificationService
    {
        Task SendOrderRequestToChef(string chefId, OrderResponseDto order);
        Task NotifyCustomerOrderAccepted(string customerId, int orderId);
        Task NotifyCustomerOrderRejected(string customerId, int orderId, string reason);
        Task NotifyChefOrderPaid(string chefId, int orderId);
        Task NotifyChefOrderCancelled(string chefId, int orderId);
        Task NotifyCustomerOrderStatusUpdate(string customerId, int orderId, Order.OrderStatus status);

        // For OrderHub to notify chefs and customers about new orders and status updates
        Task NotifyChefNewOrder(string chefId, int orderId, object payload);
        Task NotifyOrderStatusToChef(string chefId, int orderId, string status);
        Task NotifyOrderStatusToCustomer(string customerId, int orderId, string status);
        Task NotifyOrderPaid(string customerId, string chefId, int orderId, decimal amount);
        Task NotifyOrderPaymentCancelled(string customerId, string chefId, int orderId, string reason);
        Task NotifyCustomerOrderDelivered(string customerId, int orderId);

    }
}
