using Microsoft.AspNetCore.Identity;

namespace BioLife.Domain.Entity 
{
	public class AppUser : IdentityUser
	{
		public string? FullName { get; set; }
	}
	public class AppRole : IdentityRole
	{
	}
}
