using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dtos.OrderDto
{
    public class OrderItemResponseDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public decimal ItemPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public List<OrderItemAddOnResponseDto> AddOns { get; set; } = new List<OrderItemAddOnResponseDto>();
        public List<OrderItemComboResponseDto> Combos { get; set; } = new List<OrderItemComboResponseDto>();
    }
}

