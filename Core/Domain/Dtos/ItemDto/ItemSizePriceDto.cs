using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Dtos.ItemDto
{
    public class ItemSizePriceDto
    {
        public ItemSizeEnum Size { get; set; }
        public decimal Price { get; set; }
    }
}
