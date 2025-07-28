using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Identity
{
    public class User : IdentityUser
    {
        public string DisplayName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ImageUrl { get; set; }
        public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();


        // For Chefs
        //public virtual Restaurant Restaurant { get; set; }

        //// For Clients
        //public virtual ICollection<Order> Orders { get; set; }
    }

}
