using BioLife.Domain.Entities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BioLife.MVC.Models
{
	public class ProfileIndexViewModel
	{
		public string FullName { get; set; }
		public string Email { get; set; }
		public string? ProfileImage { get; set; }
		public bool TwoFactorEnabled { get; set; }
		public bool IsSubscribed { get; set; }
		public IEnumerable<Order> RecentOrders { get; set; }
	}

	public class EditProfileViewModel
	{
		[Required]
		public string FullName { get; set; }
		public IFormFile? ProfileImage { get; set; }
		public string? CurrentProfileImage { get; set; }
	}

	public class ChangeEmailViewModel
	{
		[Required]
		[EmailAddress]
		public string NewEmail { get; set; } = null!;
		[Required]
		[DataType(DataType.Password)]
		public string CurrentPassword { get; set; } = null!;
	}

	public class ChangePasswordViewModel
	{
		[Required]
		[DataType(DataType.Password)]
		public string CurrentPassword { get; set; } = null!;
		[Required]
		[DataType(DataType.Password)]
		public string NewPassword { get; set; } = null!;
		[Required]
		[DataType(DataType.Password)]
		[Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
		public string ConfirmNewPassword { get; set; } = null!;
	}


}
