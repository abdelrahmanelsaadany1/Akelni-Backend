using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dtos.OrderDto
{
    public class OrderUpdateDto
    {
        public Order.OrderStatus? Status { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? DeliveryFee { get; set; }
        public decimal? PlatformFee { get; set; }
    }
}
