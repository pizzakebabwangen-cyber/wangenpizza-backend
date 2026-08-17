using Microsoft.AspNetCore.Identity;

namespace WangenPizza.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? StripeCustomerId { get; set; }
        public string? Password { get; set; }
        public string? City { get; set; }
        public string? Street { get; set; }
        public string? Salute { get; set; }
        public string? PostBox { get; set; }
        public string? FullName { get; set; }
    }
}
