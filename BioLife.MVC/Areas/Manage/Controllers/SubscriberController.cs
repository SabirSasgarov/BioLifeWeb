using BioLife.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.ObjectModelRemoting;

namespace BioLife.MVC.Areas.Manage.Controllers
{
	[Area("Manage")]
	[Authorize(Roles = "Admin")]
	public class SubscriberController(ISubscriberService subscriberService) : Controller
	{
		public async Task<IActionResult> Index()
		{
			var subscribers = await subscriberService.GetAllSubscribersAsync();
			return View(subscribers);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			var success = await subscriberService.DeleteAsync(id);
			TempData[success ? "DeleteSuccess" : "DeleteError"] = success ?
				"Subscriber deleted successfully." : "Subscriber not found.";
			return RedirectToAction("Index");
		}
		[HttpGet]
		public async Task<IActionResult> SendEmail()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SendEmail(string subject, string message)
		{
			if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(message))
			{
				TempData["EmailError"] = "Subject and message cannot be empty.";
				return RedirectToAction("SendEmail");
			}
			await subscriberService.SendEmailToAllAsync(subject.Trim(), message.Trim());
			TempData["EmailSuccess"] = "Emails sent successfully.";
			return RedirectToAction("Index");
		}
	}
}
