using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WangenPizza.Models;

namespace WangenPizza.Controllers
{
    public class UsersController : Controller
    {
        #region ctor
        private readonly UserManager<ApplicationUser> userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }
        #endregion


        #region Index
        public IActionResult Index()
        {
            var users = userManager.Users;
            return View(users);
        }
        #endregion

        #region IndexOfAdmins
        public async Task<IActionResult> IndexOfAdmins()
        {
            var users = await userManager.GetUsersInRoleAsync("Admin");
            return View(users);
        }
        #endregion

        #region IndexOfUsers
        public async Task<IActionResult> IndexOfUsers()
        {
            var users = await userManager.GetUsersInRoleAsync("User");
            return View(users);
        }
		#endregion

		#region Delete User
		public async Task<IActionResult> Delete(string id)
		{
			var user = await userManager.FindByIdAsync(id);
			if (user == null)
			{
				return NotFound();
			}

			var result = await userManager.DeleteAsync(user);
			if (!result.Succeeded)
			{
				// Handle error
				return StatusCode(500); // Internal Server Error
			}

			// Redirect to a suitable page after deletion
			return RedirectToAction("Index", "Home");
		}
		#endregion
	}
}
