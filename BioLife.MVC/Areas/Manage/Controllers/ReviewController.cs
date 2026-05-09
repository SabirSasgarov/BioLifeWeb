using BioLife.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BioLife.MVC.Areas.Manage.Controllers
{
	[Area("Manage")]
	[Authorize(Roles = "Admin")]
	public class ReviewController(IReviewService reviewService) : Controller
	{
		private const int PageSize = 20;
		public async Task<IActionResult> Index(string? search, int page = 1)
		{
			var reviews = await reviewService.GetAllAsync(search, page, PageSize);
			var total = await reviewService.GetCountAsync(search);

			ViewBag.Search = search;
			ViewBag.Page = page;
			ViewBag.TotalPages = (int)Math.Ceiling((double)total / PageSize);
			return View(reviews);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			await reviewService.DeleteAsync(id);
			TempData["SuccessMessage"] = "Review deleted successfully.";
			return RedirectToAction(nameof(Index));
		}
	}
}
