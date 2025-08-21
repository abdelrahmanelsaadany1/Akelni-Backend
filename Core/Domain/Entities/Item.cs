using Microsoft.EntityFrameworkCore;
using Sieve.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public enum ItemSizeType
    {
        Fixed,
        Sized,
        Weighted
    }

    [Index(nameof(RestaurantId), nameof(Id))]
    [Index(nameof(CategoryId))]
    public class Item:BaseEntity
    {
       
        public string Name { get; set; }
        public string Description { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }

        [Sieve(CanFilter = true)]
        public ItemSizeType SizeType { get; set; } // Enum: Fixed, Sized, Weighted
        public int? Size { get; set; } // Nullable, only if SizeType == Sized
        public decimal? Weight { get; set; } // Nullable, only if SizeType == Weighted
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        [Sieve(CanFilter = true)]
        public int RestaurantId { get; set; }
        public virtual Restaurant? Restaurant { get; set; }

        public virtual ICollection<ItemAddOn?> ItemAddOns { get; set; }
        public virtual ICollection<ItemCombo?> ItemCombos { get; set; }
        public ICollection<ItemSizePrice> SizePricing { get; set; }

    }
}
