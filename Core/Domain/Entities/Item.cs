using Sieve.Attributes;
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

        [Sieve(CanFilter = true, CanSort = true)]
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }

        [Sieve(CanFilter = true)]
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        [Sieve(CanFilter = true)]
        public int RestaurantId { get; set; }
        public virtual Restaurant? Restaurant { get; set; }

        public virtual ICollection<ItemAddOn?> ItemAddOns { get; set; }
        public virtual ICollection<ItemCombo?> ItemCombos { get; set; }

    }
}
