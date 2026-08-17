using System.ComponentModel.DataAnnotations;

namespace WangenPizza.Api_s.Models
{
    public class ForgetPasswordModel
    {
        [EmailAddress(ErrorMessage = "Invalid Mail")]
        [Required(ErrorMessage = "This Field Required")]
        public string Email { get; set; }
    }
}
