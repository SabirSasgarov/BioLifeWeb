using BioLife.Application.Common.Interfaces;
using BioLife.Domain.Entities;
using BioLife.MVC.Helpers;
using BioLife.MVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace BioLife.MVC.Controllers
{
	public class ShopController(IProductService productService) : Controller
	{
		private readonly IProductService _productService = productService;
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
			if(product == null)
			{
				return NotFound();
			}
			return View(product);
		}
	}
}
