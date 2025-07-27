using Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Restaurant:BaseEntity
    {
      
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public double Rating { get; set; }

        public string ChefId { get; set; }
        //public virtual User Chef { get; set; }
        public virtual ICollection<Item?> Items { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Order?> Orders { get; set; }

    }
}
