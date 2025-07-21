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

        public int ComboId { get; set; }
        public virtual Combo Combo { get; set; }

    }
}
