using Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dtos.OrderDto
{
    public class OrderStatusUpdateDto
    {
        [Required]
        public Order.OrderStatus Status { get; set; }
    }
}
