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
     
        public string Name { get; set; }

      
        public virtual ICollection<Item?> Items { get; set; }
    }
}
 
        