using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Dtos.ItemDto
{
    //public class ItemClassDto
    //{

    //    public int Id { get; set; } // needed for update/get
    //    public string Name { get; set; }
    //    public string Description { get; set; }
    //    public decimal Price { get; set; }
    //    public string? ImageUrl { get; set; }

    //    public int CategoryId { get; set; }
    //    public int RestaurantId { get; set; }

    //    public List<int>? AddOnIds { get; set; }
    //    public List<int>? ComboIds { get; set; }
    //}
    public class ItemClassDto
    {
        public int Id { get; set; } // needed for update/get
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public int RestaurantId { get; set; }

        // Sizing
        public ItemSizeType SizeType { get; set; }
        public int? Size { get; set; }
        public float? Weight { get; set; }
        public List<ItemSizePriceDto>? SizePricing { get; set; }

        // AddOns
        public List<int>? AddOnIds { get; set; }
        public List<AddOnGetDto>? AddOns { get; set; }

        // Combos
        public List<int>? ComboIds { get; set; }
        public List<ComboGetDto>? Combos { get; set; }
    }

    public class AddOnGetDto
    {
        public int Id { get; set; } // needed for update/get
        public string Name { get; set; }
        public decimal AdditionalPrice { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class ComboGetDto
    {
        public int Id { get; set; } // needed for update/get
        public string Name { get; set; }
        public decimal ComboPrice { get; set; }
        public string? ImageUrl { get; set; }
    }
}
