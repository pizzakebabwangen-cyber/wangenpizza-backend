
using WangenPizza.Api_s.Models;
using WangenPizza.Helper;
using WangenPizza.Helper.Response;
using WangenPizza.Models;

namespace WangenPizza.Interfaces
{
	public interface IUserService

	{

		 Task<UserManagerResponse> RegisterUserAsync(RegisterModel model);
		Task<UserManagerResponse> LoginUserAsync(LoginModel model);
		Task<UserManagerResponse> ForgetPasswordAsync(string Email);
        Task<UserManagerResponse> ConfirmEmailAsync(string userId, string token);

        Task<UserManagerResponse> ResetPasswordAsync(ResetPasswordModel model);

		Task<UserModel> GetAccount(string id);
		Task<EditAccountCustomResponse> EditProfile(EditProfileModel model);
		Task<UserManagerResponse> EditPassword(EditPassword model);
		Task<ApplicationUser> GetUserToEdit(string name);
	}
}
