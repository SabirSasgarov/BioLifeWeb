using Microsoft.AspNetCore.Mvc;

namespace BioLife.MVC.Controllers
{
	public class CheckoutController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
