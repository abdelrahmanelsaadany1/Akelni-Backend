using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Item:BaseEntity
    {
       
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }

        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        public int RestaurantId { get; set; }
        public virtual Restaurant? Restaurant { get; set; }

        public virtual ICollection<ItemAddOn?> ItemAddOns { get; set; }
        public virtual ICollection<ItemCombo?> ItemCombos { get; set; }

    }
}
