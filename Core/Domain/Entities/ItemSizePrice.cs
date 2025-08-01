using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public enum ItemSizeEnum
    {
        Small = 0,
        Medium = 1,
        Large = 2
    }
    public class ItemSizePrice: BaseEntity
    {
        public int ItemId { get; set; }
        public ItemSizeEnum Size { get; set; } // 0: Small, 1: Medium, 2: Large
        public decimal Price { get; set; }

        public Item Item { get; set; }
    }
}
