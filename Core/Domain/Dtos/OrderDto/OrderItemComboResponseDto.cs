using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dtos.OrderDto
{
    public class OrderItemComboResponseDto
    {
        public int Id { get; set; }
        public int OrderItemId { get; set; }
        public int ComboId { get; set; }
        public string? ComboName { get; set; }
        public decimal ComboPrice { get; set; }
    }
}
