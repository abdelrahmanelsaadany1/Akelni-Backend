using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;


namespace Domain.Dtos.ItemDto
{
    public class ItemCreateUpdateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public ItemSizeType SizeType { get; set; } // Fixed, Sized, Weighted

        public int? Size { get; set; } // only if SizeType == Sized
        public float? Weight { get; set; } // only if SizeType == Weighted
        public string? ImageUrl { get; set; }

        public int CategoryId { get; set; }
        public int RestaurantId { get; set; }

        //public List<ItemSizePriceDto> SizePricing { get; set; }
        public List<int>? AddOnIds { get; set; }
        public List<int>? ComboIds { get; set; }
        public List<AddOnCreateDto>? AddOns { get; set; }
        public List<ComboCreateDto>? Combos { get; set; }
        public List<ItemSizePriceDto>? SizePricing { get; set; }
    }

    public class AddOnCreateDto
    {
        public string Name { get; set; }
        public decimal AdditionalPrice { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class ComboCreateDto
    {
        public string Name { get; set; }
        public decimal ComboPrice { get; set; }
        public string? ImageUrl { get; set; }
    }
}
