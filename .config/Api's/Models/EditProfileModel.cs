using System.ComponentModel.DataAnnotations;

namespace WangenPizza.Api_s.Models
{
	public class EditProfileModel
	{
        public string? Id { get; set; }
        public string? UserName { get; set; }
		public string? PhoneNumber { get; set; }
		public string Email { get; set; }
        public string? Street { get; set; }
        public string? Salute { get; set; }
        public string? City { get; set; }
        public string? PostBox { get; set; }

    }
}
