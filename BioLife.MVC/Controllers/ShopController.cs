using BioLife.Application.Common.Interfaces;
using BioLife.Domain.Entities;
using BioLife.Domain.Entity;
using BioLife.MVC.Helpers;
using BioLife.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BioLife.MVC.Controllers
{
	public class ShopController(IProductService productService,
		IReviewService reviewService,
		UserManager<AppUser> userManager) : Controller
	{
		private readonly IProductService _productService = productService;
		private readonly IReviewService _reviewService = reviewService;
		private const int DefaultPageSize = 12;
		public async Task<IActionResult> Index(string? search, string? category, string? sort,
			int? pageSize, int page = 1)
		{
			var size = pageSize is > 0 ? pageSize.Value : DefaultPageSize;
			page = page < 1 ? 1 : page;
			var query = _productService.GetFilteredQuery(search, category, sort);
			var products = await PaginatedList<Product>.CreateAsync(query, page, size);
			var categories = await _productService.GetCategoriesAsync();

			var vm = new ShopViewModel
			{
				Products = products,
				Categories = categories,
				Search = search,
				Category = category,
				Sort = sort,
				PageSize = size
			};
			return View(vm);
		}
		public async  Task<IActionResult> Details(int id)
		{
			var product = await _productService.GetByIdAsync(id);
			if(product == null)	return NotFound();
			
			var reviews = await _reviewService.GetByProductIdAsync(id);

			var vm = new ProductDetailViewModel
			{
				Product = product,
				Reviews = reviews.Select(r => new ReviewViewModel
				{
					UserName = r.AppUser?.UserName ?? "Unknown",
					Comment = r.Comment,
					Rating = r.Rating,
					CreatedDate = r.CreatedDate
				}).ToList()
			};

			return View(vm);
		}

		[HttpPost]
		[Authorize]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddReview(int id, AddReviewViewModel model)
		{
			if (!ModelState.IsValid)
			{
				TempData["ReviewError"] = "Please provide a valid comment and rating.";
				return RedirectToAction(nameof(Details), new { id });
			}
			var user = await userManager.GetUserAsync(User);
			if (user == null) return Unauthorized();
			var review = new Review
			{
				ProductId = id,
				UserId = user.Id,
				Comment = model.Comment,
				Rating = model.Rating,
				CreatedDate = DateTime.UtcNow
			};
			await _reviewService.AddAsync(review);
			TempData["ReviewSuccess"] = "Your review has been submitted successfully.";
			return RedirectToAction(nameof(Details), new { id });
		}

		[HttpGet]
		public async Task<IActionResult> SearchSuggestions(string? term)
		{
			if(string.IsNullOrWhiteSpace(term) || term.Length<2)
				return Json(new List<object>());

			var query = productService.GetFilteredQuery(term, null, null);
			var results = await query.Take(6).Select(p => new { p.Id, p.Name, p.Category })
				.ToListAsync();
			return Json(results);
		}
	}
}
