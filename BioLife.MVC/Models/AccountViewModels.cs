using System.ComponentModel.DataAnnotations;

namespace BioLife.MVC.Models
{
	public class LoginViewModel
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }
		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }
		public bool RememberMe { get; set; }
		public string? ReturnUrl { get; set; }
	}

	public class RegisterViewModel
	{
		[Required]
		public string FullName { get; set; }
		[Required]
		[EmailAddress]
		public string Email { get; set; }
		[Required]
		[StringLength(100, MinimumLength = 6)]
		[DataType(DataType.Password)]
		public string Password { get; set; }
		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare("Password", ErrorMessage = "Passwords do not match.")]
		public string ConfirmPassword { get; set; }

	}
}
