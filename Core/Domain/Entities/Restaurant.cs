using Domain.Entities.Identity;
using Sieve.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Restaurant:BaseEntity
    {

        [Sieve(CanSort = true)]
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }

        [Sieve(CanSort = true)]
        public double Rating { get; set; }
        public string ImageUrl { get; set; }
        public string ChefId { get; set; }
        public string OpeningHours { get; set; }
        public bool IsOpen { get; set; }

        public virtual ICollection<Item?> Items { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Order?> Orders { get; set; }

    }
}
