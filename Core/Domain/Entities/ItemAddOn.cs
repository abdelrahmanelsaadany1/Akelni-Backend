using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ItemAddOn:BaseEntity
    {
        public int ItemId { get; set; }
        public virtual Item Item { get; set; }

        public int AddOnId { get; set; }
        public virtual AddOn AddOn { get; set; }

    }
}
