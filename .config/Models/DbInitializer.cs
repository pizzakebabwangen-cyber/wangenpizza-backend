using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WangenPizza.Context;

namespace WangenPizza.Models
{
	public class DbInitializer
	{
		internal static async Task Initialize(ApplicationDbContext dbContext,
											 UserManager<ApplicationUser> userManager,
											 RoleManager<IdentityRole> roleManager
											 )
		{
			ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));

			//migrations if they are not applied
			if (dbContext.Database.GetPendingMigrations().Count() > 0)
			{
				dbContext.Database.Migrate();
			}


			await SeedUsers(userManager);


			await dbContext.SaveChangesAsync();

		}

		#region Add Admin

		private static async Task SeedUsers(UserManager<ApplicationUser> userManager)
		{
			if (await userManager.FindByNameAsync("shennawi2024") != null)
				return;


			var user = new ApplicationUser
			{
				Email = "shennawi2024@gmail.com",
				UserName = "shennawi2024",
				Password = "@shennawi",
				EmailConfirmed = true,
			};
			await userManager.CreateAsync(user, "@shennawi");
			await userManager.AddToRoleAsync(user, "Admin");

		}

		#endregion
	}

}
