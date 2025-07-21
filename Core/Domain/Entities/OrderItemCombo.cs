using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class OrderItemCombo:BaseEntity
    {
        public int OrderItemId { get; set; }
        public virtual OrderItem OrderItem { get; set; }

        public int ComboId { get; set; }
        public virtual Combo Combo { get; set; }

    }
}
