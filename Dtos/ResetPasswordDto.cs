using System.ComponentModel.DataAnnotations;

namespace WangenPizza.Dtos
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "This Field Required")]
        public string Password { get; set; }

        [Required(ErrorMessage = "This Field Required")]
        [Compare("Password", ErrorMessage = "Password Not Match")]
        public string ConfirmPassword { get; set; }

        public string? Email { get; set; }
        public string? Token { get; set; }
    }
}
