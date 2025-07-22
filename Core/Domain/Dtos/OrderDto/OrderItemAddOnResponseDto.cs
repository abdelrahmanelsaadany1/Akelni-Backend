using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dtos.OrderDto
{
    public class OrderItemAddOnResponseDto
    {
        public int Id { get; set; }
        public int OrderItemId { get; set; }
        public int AddOnId { get; set; }
        public string? AddOnName { get; set; }
        public decimal AddOnPrice { get; set; }
    }
}
