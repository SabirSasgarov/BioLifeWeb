namespace BioLife.Persistence
{
	public static class DataSeed
	{
		public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
		{
			var roleManager = serviceProvider.GetRequiredService<RoleManager<AppRole>>();
			var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
			string[] roleNames = { "Admin", "Member" };

			foreach (var roleName in roleNames)
			{
				if(!await roleManager.RoleExistsAsync(roleName))
				{
					await roleManager.CreateAsync(new AppRole { Name = roleName });
				}
			}
			var adminEmail = "admin@biolife.com";
			var adminUser = await userManager.FindByEmailAsync(adminEmail);
			if (adminUser == null)
			{
				var newAdmin = new AppUser
				{
					UserName = adminEmail,
					Email = adminEmail,
					EmailConfirmed = true,
					FullName = "Admin"
				};

				var result = await userManager.CreateAsync(newAdmin, "Admin123!");
				
				if (result.Succeeded)
				{
					await userManager.AddToRoleAsync(newAdmin, "Admin");
				}
			}
		}
	}
}
