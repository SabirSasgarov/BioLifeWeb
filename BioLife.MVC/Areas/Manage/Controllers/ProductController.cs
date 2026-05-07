using BioLife.Application.Common.Interfaces;
using BioLife.Domain.Entities;
using BioLife.MVC.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BioLife.MVC.Areas.Manage.Controllers
{
	[Area("Manage")]
	[Authorize(Roles = "Admin")]
	public class ProductController(IProductService productService,
		IWebHostEnvironment webHostEnvironment) : Controller
	{
		private const int AdminPageSize = 15;
		private readonly IProductService _productService;
		public async Task<IActionResult> Index(string? search, string? category, string? sort,
			int? pageSize, int page = 1)
		{
			var query = productService.GetFilteredQuery(search, category, sort);
			var pagedProducts = await PaginatedList<Product>.CreateAsync(query, page, AdminPageSize);
			var categories = await productService.GetCategoriesAsync();
			
			ViewBag.Search = search;
			ViewBag.Categories = categories;
			ViewBag.Sort = sort;
			
			return View(pagedProducts);
		}
		public IActionResult Create()
		{
			return View();
		}
		//[HttpPost]
		//[ValidateAntiForgeryToken]
		//public async Task<IActionResult> Create(ProductCreateViewModel)
		//{
		//	return RedirectToAction();
		//}
	}
}
