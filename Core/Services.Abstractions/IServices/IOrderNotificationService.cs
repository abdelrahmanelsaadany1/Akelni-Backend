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
    }
}
