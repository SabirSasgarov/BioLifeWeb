using Microsoft.AspNetCore.Mvc;

namespace BioLife.MVC.Controllers
{
	public class ShopController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
		public IActionResult Details()
		{
			return View();
		}
	}
}
