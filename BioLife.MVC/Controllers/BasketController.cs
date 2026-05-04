using Microsoft.AspNetCore.Mvc;

namespace BioLife.MVC.Controllers
{
	public class BasketController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
