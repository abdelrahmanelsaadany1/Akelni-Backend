using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dtos.OrderDto
{
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public Order.OrderStatus Status { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal TotalAmount => SubTotal + DeliveryFee + PlatformFee;
        public double DistanceKm { get; set; }
        public string CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int RestaurantId { get; set; }
        public string? RestaurantName { get; set; }
        public List<OrderItemResponseDto> Items { get; set; } = new List<OrderItemResponseDto>();
        public PaymentResponseDto? Payment { get; set; }
    }
}
