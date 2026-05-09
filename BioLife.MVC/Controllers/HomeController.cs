using BioLife.MVC.Models;
using BioLife.Application.Common.Interfaces;
using BioLife.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BioLife.MVC.Controllers
{
	public class HomeController(IProductService productService, ISubscriberService subscriberService, IEmailService emailService, IWebHostEnvironment env) : Controller
	{
		public async Task<IActionResult> Index()
		{
			var products = await productService.GetAllAsync();
			return View(products);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Subscribe(string email)
		{
			if(string.IsNullOrWhiteSpace(email))
			{
				TempData["SubscriptionError"] = "Please enter a valid email address.";
				return RedirectToAction("Index");
			}

			var success = await subscriberService.SubscribeAsync(email.Trim().ToLower());
			TempData[success ? "SubscriptionSuccess" : "SubscriptionError"] = success
				? "Subscription successful!" : "You are already subscribed.";

			if (success)
			{
				var templatePath = Path.Combine(env.WebRootPath, "templates", "SubscriptionSuccess.html");
				var emailBody = System.IO.File.Exists(templatePath) 
					? await System.IO.File.ReadAllTextAsync(templatePath)
					: "Thank you for subscribing to our newsletter!";

				await emailService.SendEmailAsync(email.Trim().ToLower(), "Welcome to BioLife Newsletter!", emailBody);
			}

			return RedirectToAction("Index");
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Unsubscribe(string email)
		{
			await subscriberService.UnsubscribeAsync(email);
			return RedirectToAction("Index");
		}

		[Route("/Home/Error404")]
		public IActionResult Error404()
		{
			return View();
		}
	}
}
