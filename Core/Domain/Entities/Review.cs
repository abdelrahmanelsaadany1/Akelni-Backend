using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Review : BaseEntity
    {
        public int RestaurantId { get; set; }
        public virtual Restaurant Restaurant { get; set; }
        public string UserId { get; set; }
        public int Rating { get; set; } // e.g., 1-5
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
