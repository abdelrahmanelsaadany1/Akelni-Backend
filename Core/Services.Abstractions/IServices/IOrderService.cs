// In Services.Abstractions/IOrderService.cs (or similar path)

using Domain.Dtos.OrderDto; // Assuming your DTOs are here
using Domain.Entities;     // Assuming your Entities are here

namespace Services.Abstractions.ICategoryService // Adjust namespace as needed
{
    public interface IOrderService
    {
        Task AddOrderAsync(Order order, List<OrderItemCreateDto> orderItems, int totalAmount);
        Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync();
        Task<OrderResponseDto> GetOrderByIdAsync(int id);
        Task<IEnumerable<OrderResponseDto>> GetOrdersByCustomerIdAsync(string customerId);
        Task<IEnumerable<OrderResponseDto>> GetOrdersByRestaurantIdAsync(int restaurantId);
        Task<IEnumerable<OrderResponseDto>> GetOrdersByStatusAsync(Order.OrderStatus status);
        Task UpdateOrderStatusAsync(int id, Order.OrderStatus status);
        Task DeleteOrderAsync(int id);
        Task<IEnumerable<OrderResponseDto>> GetCurrentUserOrdersAsync();
        Task<decimal> CalculateOrderTotalAsync(int orderId);
        // Add other order-specific business methods here
    }
}