using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dtos.OrderDto
{
    public class OrderSummaryDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public Order.OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string CustomerId { get; set; }
        public int RestaurantId { get; set; }
        public string? RestaurantName { get; set; }
        public int ItemCount { get; set; }
    }
}
