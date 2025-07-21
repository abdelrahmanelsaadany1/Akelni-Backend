using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dtos.Auth
{
    public class RegisterDto
    {
        [Required, EmailAddress] 
        public string Email { get; set; }
        [Required] 
        public string Password { get; set; }
        [Required] 
        public string Role { get; set; } // "Admin", "Customer", "Chef"
    }

}
