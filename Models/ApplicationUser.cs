using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestDemo.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        
        public string Password { get; set; }
        public string Country { get; set; }
        public string Bio { get; set; }
    }
}
