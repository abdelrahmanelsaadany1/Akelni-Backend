using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Identity
{
    public class User:IdentityUser
    {
       public string DisplayName {  get; set; }
        public Address Address { get; set; }


        // For Chefs
        //public virtual Restaurant Restaurant { get; set; }

        //// For Clients
        //public virtual ICollection<Order> Orders { get; set; }
    }

}
