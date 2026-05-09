using BioLife.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace BioLife.Domain.Entity 
{
	public class AppUser : IdentityUser
	{
		public string? FullName { get; set; }
		public string? ProfileImage { get; set; }
		public List<Review> Reviews { get; set; }
	}
	public class AppRole : IdentityRole
	{
	}
}
