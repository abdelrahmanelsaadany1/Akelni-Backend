using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dtos.OrderDto
{
    public class OrderItemCreateDto
    {
        [Required]
        public int ItemId { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000.")]
        public int Quantity { get; set; }

        public List<int> AddOnIds { get; set; } = new List<int>();
        public List<int> ComboIds { get; set; } = new List<int>();
    }
}
