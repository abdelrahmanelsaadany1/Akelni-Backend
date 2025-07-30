using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class OrderItem:BaseEntity
    {
     
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        public int ItemId { get; set; }
        public virtual Item Item { get; set; }
        public int Quantity { get; set; }
        public decimal ItemPrice { get; set; } // ← stored from Item.Price
        public decimal TotalPrice { get; set; } // ← ItemPrice * Quantity + AddOns + Combos

        public virtual ICollection<OrderItemAddOn> AddOns { get; set; }
        public virtual ICollection<OrderItemCombo> Combos { get; set; }

    }
}
