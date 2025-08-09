using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dtos.OrderDto
{
    public class OrderCreateDto
    {
        [Required]
        public int RestaurantId { get; set; }

        [Required]
        public List<OrderItemCreateDto> Items { get; set; } = new List<OrderItemCreateDto>();

        //[Required]
        [Range(0, double.MaxValue, ErrorMessage = "Distance must be a positive value")]
        public double DistanceKm { get; set; }

        [Required, Range(0, double.MaxValue, ErrorMessage ="Total amount is not valid")]
        public double amount { get; set; }
    }
}
