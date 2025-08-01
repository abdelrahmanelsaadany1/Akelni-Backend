using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ItemCombo:BaseEntity
    {
        public int ItemId { get; set; }
        public virtual Item Item { get; set; }

        public decimal ComboPrice { get; set; } // Price for the add-on when added to an item
        public string? ImageUrl { get; set; } // Optional image URL for the add-on

        public int ComboId { get; set; }
        public virtual Combo Combo { get; set; }

    }
}
