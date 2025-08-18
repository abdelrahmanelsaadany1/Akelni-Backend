using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dtos.OrderReportsDto
{
    public  class OrderDto
    {
        public int OrderId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public decimal DeliveryFee { get; set; }
        public decimal PlatformFee { get; set; }
        public double DistanceKm { get; set; }
        public decimal Total { get; set; }
    }
}
