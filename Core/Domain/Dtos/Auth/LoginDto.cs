﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dtos.Auth
{
    public class LoginDto
    {
        [Required, EmailAddress] 
        public string Email { get; set; }
        [Required] 
        public string Password { get; set; }
        public string? rememberMe { get; set; }
    }

}
