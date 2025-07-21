using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class OrderItemAddOn:BaseEntity
    {
        public int OrderItemId { get; set; }
        public virtual OrderItem OrderItem { get; set; }

        public int AddOnId { get; set; }
        public virtual AddOn AddOn { get; set; }

    }
}
