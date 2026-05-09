using BioLife.Application.Common.Interfaces;
using BioLife.Domain.Entity;
using BioLife.MVC.Models;
using BioLife.MVC.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;


namespace BioLife.MVC.Controllers
{
	public class AccountController : Controller
	{
		private const string TwoFactorRememberOnceCachePrefix = "2FA_RememberOnce_";
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;
		private readonly RoleManager<AppRole> _roleManager;
		private readonly IEmailService _emailService;
		private readonly IWebHostEnvironment _env;
		private readonly IMemoryCache _cache;
		private readonly IAccountService _accountService;
		private readonly IBasketService _basketService;

		public AccountController(UserManager<AppUser> userManager,
			SignInManager<AppUser> signInManager,
			RoleManager<AppRole> roleManager,
			IEmailService emailService,
			IWebHostEnvironment env,
			IMemoryCache cache,
			IAccountService accountService,
			IBasketService basketService)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_roleManager = roleManager;
			_emailService = emailService;
			_env = env;
			_cache = cache;
			_accountService = accountService;
			_basketService = basketService;
		}
		private async Task SetBasketCountCookie(string userId)
		{
			var count = await _basketService.GetBasketCountAsync(userId);
			Response.Cookies.Append("BasketCount", count.ToString(), new CookieOptions
			{
				Expires = DateTimeOffset.UtcNow.AddDays(30),
				HttpOnly = false,
				Secure = true,
				SameSite = SameSiteMode.Strict
			});
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
			if (!ModelState.IsValid)
				return View(model);

			var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
			if (result.IsLockedOut)
			{
				ModelState.AddModelError(string.Empty, "Your account is locked due to multiple failed login attempts. Please try again later.");
				return View(model);
			}

			if (result.Succeeded)
			{
				var user = await _userManager.FindByEmailAsync(model.Email);
				if (user != null)
				{
					var roles = await _userManager.GetRolesAsync(user);
					bool isAdmin = roles.Contains("Admin");
					if (!isAdmin)
					{
						if (!TryConsumeTwoFactorRememberOnce(user.Id))
						{
							await _signInManager.SignOutAsync();
							var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

							var sessionToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
							var codeHash = Convert.ToBase64String(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(code)));

							var cacheKey = $"2FA_{user.Id}_{sessionToken}";
							_cache.Set(cacheKey, new TwoFactorCacheEntry
							{ CodeHash = codeHash, Attempts = 0 }, TimeSpan.FromMinutes(5));

							await _emailService.SendEmailAsync(user.Email,
								"Your BioLife 2FA Code", $"Your 2FA code is: {code}. You have 5 minutes to use it");

							TempData["2FA_UserId"] = user.Id;
							TempData["2FA_Session"] = sessionToken;
							TempData["2FA_ReturnUrl"] = returnUrl;
							TempData["2FA_RememberMe"] = model.RememberMe;
							return RedirectToAction("TwoFactorVerify", new { returnUrl, rememberMe = model.RememberMe });
						}

						if (model.RememberMe)
						{
							SetTwoFactorRememberOnce(user.Id);
						}

						await SetBasketCountCookie(user.Id);

						if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
						{
							return Redirect(returnUrl);
						}
					}

					if (isAdmin)
						return RedirectToAction("Index", "Dashboard", new { area = "Manage" });

					return RedirectToAction("Index", "Home");
				}
			}
			ModelState.AddModelError(string.Empty, "Invalid login attempt.");
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
				await SetBasketCountCookie(user.Id);

				if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
				{
					return LocalRedirect(returnUrl);
				}
				return RedirectToAction("Index", "Home");
			}
			TempData["ErrorMessage"] = "Email claim not received from external provider.";
			return RedirectToAction("Login");
		}

		[HttpGet]
		public IActionResult TwoFactorVerify(string? returnUrl = null, bool rememberMe = false)
		{
			if (TempData["2FA_UserId"] == null)
			{
				return RedirectToAction("Login");
			}
			TempData.Keep("2FA_UserId");
			TempData.Keep("2FA_Session");
			TempData.Keep("2FA_ReturnUrl");
			TempData.Keep("2FA_RememberMe");

			return View(new TwoFactorViewModel { ReturnUrl = returnUrl, RememberMe = rememberMe });
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ResendTwoFactorCode()
		{
			var userId = TempData["2FA_UserId"]?.ToString();
			var sessionToken = TempData["2FA_Session"]?.ToString();
			var returnUrl = TempData["2FA_ReturnUrl"]?.ToString();
			var rememberMe = TempData["2FA_RememberMe"] is bool rm && rm;
			if (userId == null)
				return RedirectToAction("Login");

			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
				return RedirectToAction("Login");

			if (await _userManager.IsLockedOutAsync(user))
			{
				var lockendOutEnd = await _userManager.GetLockoutEndDateAsync(user);
				var minutesLeft = lockendOutEnd.HasValue ? (int)Math.Ceiling((lockendOutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes) : 15;
				TempData["ErrorMessage"] = $"Your account is locked due to multiple failed 2FA attempts. Please try again after {minutesLeft} minutes.";
				return RedirectToAction("Login");
			}
			if (sessionToken != null)
			{
				_cache.Remove($"2FA_{userId}_{sessionToken}");
			}
			var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
			var newSessionToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
			var codeHash = Convert.ToBase64String(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(code)));
			var cacheKey = $"2FA_{user.Id}_{newSessionToken}";

			_cache.Set(cacheKey, new TwoFactorCacheEntry
			{ CodeHash = codeHash, Attempts = 0 }, TimeSpan.FromMinutes(5));
			await _emailService.SendEmailAsync(user.Email,
				"Your BioLife 2FA Code", $"Your 2FA code is: {code}. You have 5 minutes to use it");
			TempData["2FA_UserId"] = user.Id;
			TempData["2FA_Session"] = newSessionToken;
			TempData["2FA_ReturnUrl"] = returnUrl;
			TempData["2FA_RememberMe"] = rememberMe;
			TempData["SuccessMessage"] = "A new 2FA code has been sent to your email.";
			return RedirectToAction("TwoFactorVerify", new { returnUrl, rememberMe });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> TwoFactorVerify(TwoFactorViewModel model)
		{
			var userId = TempData["2FA_UserId"].ToString();
			var sessionToken = TempData["2FA_Session"].ToString();

			if (userId == null && sessionToken == null)
			{
				return RedirectToAction("Login");
			}
			if (!ModelState.IsValid)
			{
				TempData.Keep("2FA_UserId");
				TempData.Keep("2FA_Session");
				return View(model);
			}
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return RedirectToAction("Login");
			}

			var cacheKey = $"2FA_{userId}_{sessionToken}";

			if (!_cache.TryGetValue(cacheKey, out TwoFactorCacheEntry? entry))
			{
				ModelState.AddModelError(string.Empty, "Your 2FA code has been expired. Please login again");
			}
			const int maxAttempts = 5;
			if (entry!.Attempts >= maxAttempts)
			{
				_cache.Remove(cacheKey);
				await _userManager.SetLockoutEnabledAsync(user, true);
				await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(15));
				ModelState.AddModelError(string.Empty, "You have exceeded the maximum number of 2FA attemts." +
					"Please try again after 15 minutes.");
				return RedirectToAction("Login");
			}
			var submittedHash = Convert.ToBase64String(SHA256
				.HashData(System.Text.Encoding.UTF8.GetBytes(model.Code.Trim())));
			if (submittedHash != entry!.CodeHash)
			{
				_cache.Set(cacheKey, entry with { Attempts = entry.Attempts + 1 }, TimeSpan.FromMinutes(5));
				TempData.Keep("2FA_UserId");
				TempData.Keep("2FA_Session");
				var remaining = maxAttempts - entry.Attempts - 1;
				ModelState.AddModelError(string.Empty, $"Invalid 2FA code. You have {remaining} attempts remaining.");
				return View(model);
			}
			_cache.Remove(cacheKey);
			await _signInManager.SignInAsync(user, model.RememberMe);
			await SetBasketCountCookie(user.Id);
			if (model.RememberMe)
			{
				SetTwoFactorRememberOnce(user.Id);
			}
			if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
			{
				return RedirectToAction(model.ReturnUrl);
			}

			return RedirectToAction("Index", "Home");
		}

		private bool TryConsumeTwoFactorRememberOnce(string userId)
		{
			var cacheKey = $"{TwoFactorRememberOnceCachePrefix}{userId}";
			if (_cache.TryGetValue(cacheKey, out _))
			{
				_cache.Remove(cacheKey);
				return true;
			}

			return false;
		}

		private void SetTwoFactorRememberOnce(string userId)
		{
			var cacheKey = $"{TwoFactorRememberOnceCachePrefix}{userId}";
			_cache.Set(cacheKey, true, TimeSpan.FromDays(7));
		}

		[HttpGet]
		public IActionResult ForgotPassword()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var linkTemplate = Url.Action("ResetPassword", "Account", new { token = "{{Token}}", email = model.Email }, Request.Scheme);
				if (linkTemplate != null)
				{
					await _accountService.RequestPasswordResetAsync(model.Email, linkTemplate);
				}

				return RedirectToAction("ForgotPasswordConfirmation");
			}
			return View(model);
		}

		[HttpGet]
		public IActionResult ForgotPasswordConfirmation()
		{
			return View();
		}
		[HttpGet]
		public IActionResult ResetPassword(string email, string token)
		{
			var model = new ResetPasswordViewModel { Email = email, Token = token };
			if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
			{
				ModelState.AddModelError(string.Empty, "Invalid password reset token.");
			}

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var result = await _accountService.ResetPasswordAsync(model.Email, model.Token, model.Password);
			if (result.Succeeded)
			{
				return RedirectToAction("Login");
			}
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}
			return View(model);

		}
	}
}
