using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]

        public string FullName { get; set; }

        public const string AdminRole = "Admin";
        public const string CustomerRole = "Customer";

        

    }
}
