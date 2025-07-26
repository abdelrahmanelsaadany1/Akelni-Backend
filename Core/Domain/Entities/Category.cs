using Sieve.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Category:BaseEntity
    {
        [Sieve(CanFilter = true, CanSort = true)]
           public string Name { get; set; }

      
        public virtual ICollection<Item?> Items { get; set; }
    }
}
 
        