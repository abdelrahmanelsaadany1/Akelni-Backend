using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class AddOn:BaseEntity
    {
       
        public string Name { get; set; }
        public decimal AdditionalPrice { get; set; }
        public string? ImageUrl { get; set; } // Optional image URL for the add-on

        public int? RestaurantId { get; set; } // Foreign key to the restaurant
        public virtual Restaurant? Restaurant { get; set; } // Navigation property to the restaurant

        public virtual ICollection<ItemAddOn?> ItemAddOns { get; set; }
        public virtual ICollection<OrderItemAddOn?> OrderItemAddOns { get; set; }

    }
}
