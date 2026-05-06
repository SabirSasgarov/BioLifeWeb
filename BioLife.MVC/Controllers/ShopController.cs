using BioLife.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BioLife.MVC.Controllers
{
	public class ShopController(IProductService productService) : Controller
	{
		private readonly IProductService _productService = productService;

		public async Task<IActionResult> Index()
		{
			var products = await _productService.GetAllAsync();
			return View(products);
		}
		public IActionResult Details()
		{
			return View();
		}
	}
}
