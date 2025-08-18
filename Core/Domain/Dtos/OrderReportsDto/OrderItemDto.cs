using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dtos.OrderReportsDto
{
    public  class OrderItemDto
    {
        public int OrderId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal ItemPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
