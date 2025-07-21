using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Combo:BaseEntity
    {
     
        public string Name { get; set; }
        public decimal ComboPrice { get; set; }

        public virtual ICollection<ItemCombo> ItemCombos { get; set; }
        public virtual ICollection<OrderItemCombo> OrderItemCombos { get; set; }

    }
}
