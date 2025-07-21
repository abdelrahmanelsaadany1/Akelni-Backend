using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class AddOn:BaseEntity
    {
       
        public string Name { get; set; }
        public decimal AdditionalPrice { get; set; }

        public virtual ICollection<ItemAddOn> ItemAddOns { get; set; }
        public virtual ICollection<OrderItemAddOn> OrderItemAddOns { get; set; }

    }
}
