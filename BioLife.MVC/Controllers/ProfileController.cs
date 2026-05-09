using BioLife.Application.Common.Interfaces;
using BioLife.Domain.Entity;
using BioLife.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BioLife.MVC.Controllers
{
	[Authorize]
	public class ProfileController(
		UserManager<AppUser> userManager,
		SignInManager<AppUser> singInManager,
		IOrderService orderService,
		ISubscriberService subscriberService,
		IEmailService emailService,
		IWebHostEnvironment env) : Controller
	{
		public async Task<IActionResult> Index()
		{
			var user = await userManager.GetUserAsync(User);
			if (user == null) return NotFound();

			var orders = await orderService.GetOrderByUserAsync(user.Id);
			var isSubscribed = await subscriberService.IsSubscribedAsync(user.Email);

			var vm = new Models.ProfileIndexViewModel
			{
				FullName = user.FullName ?? user.UserName!,
				Email = user.Email!,
				ProfileImage = user.ProfileImage,
				TwoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user),
				IsSubscribed = isSubscribed,
				RecentOrders = orders.Take(5)
			};
			return View(vm);
		}
		[HttpGet]
		public async Task<IActionResult> Edit()
		{
			var user = await userManager.GetUserAsync(User);
			if (user == null) return NotFound();

			return View(new EditProfileViewModel
			{
				FullName = user.FullName ?? user.UserName!,
				CurrentProfileImage = user.ProfileImage
			});
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(EditProfileViewModel vm)
		{
			if (!ModelState.IsValid) return View(vm);
			var user = await userManager.GetUserAsync(User);
			if (user == null) return NotFound();
			user.FullName = vm.FullName;
			if (vm.ProfileImage != null && vm.ProfileImage.Length > 0)
			{
				var allowed = new[] { ".jpeg", ".png", ".gif", ".jpg", ".bmp", ".webp" };
				var ext = Path.GetExtension(vm.ProfileImage.FileName).ToLowerInvariant();
				if (!allowed.Contains(ext))
				{
					ModelState.AddModelError("ProfileImage", "Invalid file type. Allowed types: " + string.Join(", ", allowed));
					vm.CurrentProfileImage = user.ProfileImage;
					return View(vm);
				}
				var avatarDir = Path.Combine(env.WebRootPath, "uploads", "avatars");
				Directory.CreateDirectory(avatarDir);

				if (!string.IsNullOrEmpty(user.ProfileImage))
				{
					var oldPath = Path.Combine(env.WebRootPath, user.ProfileImage.TrimStart('/'));
					if (System.IO.File.Exists(oldPath))
					{
						System.IO.File.Delete(oldPath);
					}
				}
				var fileName = $"{Guid.NewGuid()}{ext}";
				var filePath = Path.Combine(avatarDir, fileName);
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await vm.ProfileImage.CopyToAsync(stream);
				}
				user.ProfileImage = $"/uploads/avatars/{fileName}";
			}

			var result = await userManager.UpdateAsync(user);
			if (result.Succeeded)
			{
				await singInManager.RefreshSignInAsync(user);
				TempData["SuccessMessage"] = "Profile updated successfully.";
				return RedirectToAction("Index");
			}
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
				vm.CurrentProfileImage = user.ProfileImage;
			}
			return View(vm);
		}
		[HttpGet]
		public IActionResult ChangeEmail() => View(new ChangeEmailViewModel());

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel vm)
		{
			if (!ModelState.IsValid) return View(vm);
			var user = await userManager.GetUserAsync(User);
			if (user == null) return NotFound();
			var passwordCheck = await userManager.CheckPasswordAsync(user, vm.CurrentPassword);
			if (!passwordCheck)
			{
				ModelState.AddModelError("CurrentPassword", "Incorrect password.");
				return View(vm);
			}

			var existingUser = await userManager.FindByEmailAsync(vm.NewEmail);
			if (existingUser != null && existingUser.Id != user.Id)
			{
				ModelState.AddModelError("NewEmail", "Email is already in use.");
				return View(vm);
			}

			var token = await userManager.GenerateChangeEmailTokenAsync(user, vm.NewEmail);
			var confirmLink = Url.Action("ConfirmEmailChange", "Profile",
				new { userId = user.Id, email = vm.NewEmail, token }, Request.Scheme);
			await emailService.SendEmailAsync(vm.NewEmail, "Confirm your email change",
				$"Please confirm your email change by clicking <a href='{confirmLink}'>here</a>.");
			TempData["SuccessMessage"] = "Confirmation email sent. Please check your inbox.";
			return RedirectToAction(nameof(Index));
		}

		[HttpGet]
		public async Task<IActionResult> ConfirmEmailChange(string userId, string email, string token)
		{
			if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
				return NotFound();

			var user = await userManager.FindByIdAsync(userId);
			if (user == null) return NotFound();
			var result = await userManager.ChangeEmailAsync(user, email, token);
			if (!result.Succeeded)
			{
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
				return View();
			}
			user.UserName = email;
			await userManager.UpdateNormalizedEmailAsync(user);
			await userManager.UpdateNormalizedUserNameAsync(user);
			await userManager.UpdateAsync(user);
			await singInManager.RefreshSignInAsync(user);
			TempData["SuccessMessage"] = "Email changed successfully.";
			return RedirectToAction(nameof(Index));
		}

		[HttpGet]
		public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
		{
			if (!ModelState.IsValid) return View(vm);
			var user = await userManager.GetUserAsync(User);
			if (user == null) return NotFound();
			var result = await userManager.ChangePasswordAsync(user, vm.CurrentPassword, vm.NewPassword);
			if (result.Succeeded)
			{
				await singInManager.RefreshSignInAsync(user);
				TempData["SuccessMessage"] = "Password changed successfully.";
				return RedirectToAction(nameof(Index));
			}
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}
			return View(vm);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ToggleTwoFA()
		{
			var user = await userManager.GetUserAsync(User);
			if (user == null) return NotFound();
			var current = await userManager.GetTwoFactorEnabledAsync(user);
			await userManager.SetTwoFactorEnabledAsync(user, !current);

			TempData["SuccessMessage"] = $"Two-factor authentication {(current ? "enabled" : "disabled")} successfully.";

			return RedirectToAction(nameof(Index));
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ToggleSubscription()
		{
			var user = await userManager.GetUserAsync(User);
			if (user == null) return NotFound();
			var isSubscribed = await subscriberService.IsSubscribedAsync(user.Email!);
			if (isSubscribed)
			{
				await subscriberService.UnsubscribeAsync(user.Email!);
				TempData["SuccessMessage"] = "Unsubscribed from newsletter successfully.";
			}
			else
			{
				await subscriberService.SubscribeAsync(user.Email!);
				TempData["SuccessMessage"] = "Subscribed to newsletter successfully.";
			}
			return RedirectToAction(nameof(Index));
		}
		public async Task<IActionResult> Orders()
		{
			var user = await userManager.GetUserAsync(User);
			if (user == null) return NotFound();
			var orders = await orderService.GetOrderByUserAsync(user.Id);
			return View(orders);
		}

		public async Task<IActionResult> OrderDetails(int id)
		{
			var user = await userManager.GetUserAsync(User);
			if (user == null) return NotFound();
			var order = await orderService.GetOrderByIdAsync(id);
			if (order == null || order.AppUserId != user.Id) return NotFound();
			return View(order);
		}
	}
}
