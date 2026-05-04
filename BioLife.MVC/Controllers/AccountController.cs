using BioLife.Application.Common.Interfaces;
using BioLife.Domain.Entity;
using BioLife.MVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Security.Claims;

namespace BioLife.MVC.Controllers
{
	public class AccountController : Controller
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;
		private readonly RoleManager<AppRole> _roleManager;
		private readonly IEmailService _emailService;
		private readonly IWebHostEnvironment _env;

		public AccountController(UserManager<AppUser> userManager,
			SignInManager<AppUser> signInManager,
			RoleManager<AppRole> roleManager,
			IEmailService emailService,
			IWebHostEnvironment env)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_roleManager = roleManager;
			_emailService = emailService;
			_env = env;
		}

		[HttpGet]
		public IActionResult Login(string? returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;
			if (ModelState.IsValid)
			{
				var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
				if (result.Succeeded)
				{
					var user = await _userManager.FindByEmailAsync(model.Email);
					if (user != null)
					{
						var roles = await _userManager.GetRolesAsync(user);
						if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
						{
							return RedirectToAction(returnUrl);
						}
						if (roles.Contains("Admin"))
							return RedirectToAction("Index", "Home", new { area = "Manage" });

						return RedirectToAction("Index", "Home");
					}
				}
				ModelState.AddModelError(string.Empty, "Invalid login attempt.");
			}
			return View(model);
		}

		[HttpGet]
		public IActionResult Register()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = new AppUser { UserName = model.Email, Email = model.Email, FullName = model.FullName };
				var result = await _userManager.CreateAsync(user, model.Password);

				var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

				var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token }, Request.Scheme);

				if (result.Succeeded)
				{
					if (!await _roleManager.RoleExistsAsync("Member"))
					{
						await _roleManager.CreateAsync(new AppRole { Name = "Member" });
					}
					await _userManager.AddToRoleAsync(user, "Member");
					//await _signInManager.SignInAsync(user, isPersistent: false);

					var templatePath = Path.Combine(_env.WebRootPath, "templates", "email-confirmation.html");
					var emailBody = await System.IO.File.ReadAllTextAsync(templatePath);
					emailBody = emailBody.Replace("{{ConfirmationLink}}", confirmationLink);

					await _emailService.SendEmailAsync(user.Email,
					"Welcome to BioLife - Confirm your Email",
					emailBody);

					return RedirectToAction("Login");
				}
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}



			return View(model);
		}

		[HttpGet]
		public async Task<IActionResult> ConfirmEmail(string userId, string token)
		{
			if (userId == null || token == null)
			{
				return RedirectToAction("Index", "Home");
			}
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return NotFound($"Unable to load user with ID '{userId}'.");
			}

			if (user.EmailConfirmed)
			{
				ModelState.AddModelError(string.Empty, "Email is already confirmed.");
				return View("ConfirmEmail");
			}

			var result = await _userManager.ConfirmEmailAsync(user, token);
			if (result.Succeeded)
			{
				await _userManager.UpdateSecurityStampAsync(user);
				return View("ConfirmEmail");
			}
			else
			{
				ModelState.AddModelError(string.Empty, "Invalid or expired confirmation link.");
				return RedirectToAction("Error404", "Home");
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult ExternalLogin(string provider, string? returnUrl = null)
		{
			var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
			var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
			return Challenge(properties, provider);
		}


		public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
		{
			returnUrl = returnUrl ?? Url.Content("~/");
			if (remoteError != null)
			{
				ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
				return View("Login");
			}
			var info = await _signInManager.GetExternalLoginInfoAsync();
			if (info == null)
			{
				TempData["ErrorMessage"] = "Error loading external login information.";
				return RedirectToAction("Login");
			}
			var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
			if (signInResult.IsNotAllowed)
			{
				TempData["ErrorMessage"] = "Your account is not allowed to sign in.";
				return RedirectToAction("Login");
			}

			var email = info.Principal.FindFirstValue(ClaimTypes.Email);
			var name = info.Principal.FindFirstValue(ClaimTypes.Name);

			if (email != null)
			{
				var user = await _userManager.FindByEmailAsync(email);
				if (user == null)
				{
					user = new AppUser { UserName = email, Email = email, FullName = name, EmailConfirmed = true };
					var createResult = await _userManager.CreateAsync(user);

					if (createResult.Succeeded)
					{
						if (!await _roleManager.RoleExistsAsync("Member"))
							await _roleManager.CreateAsync(new AppRole { Name = "Member" });

						await _userManager.AddToRoleAsync(user, "Member");
					}

				}
				await _userManager.AddLoginAsync(user, info);
				await _signInManager.SignInAsync(user, isPersistent: false);

				if(!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
				{
					return LocalRedirect(returnUrl);
				}
				return RedirectToAction("Index", "Home");
			}
			TempData["ErrorMessage"] = "Email claim not received from external provider.";
			return RedirectToAction("Login");
		}
	}
}
