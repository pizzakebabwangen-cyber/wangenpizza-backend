using System.ComponentModel.DataAnnotations;

namespace WangenPizza.Api_s.Models
{
    public class RegisterModel
    {
    //    public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
		[EmailAddress(ErrorMessage = "Invalid Mail")]
		[Required(ErrorMessage = "This Field Required")]
		public string Email { get; set; }
        public string Password { get; set; }
		public string ConfirmPassword { get; set; }
        public string? Street { get; set; }
        public string? Salute { get; set; }
        public string? City { get; set; }
		public string? PostBox { get; set; }



	}
}
