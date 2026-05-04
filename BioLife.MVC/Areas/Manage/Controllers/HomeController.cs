using Microsoft.AspNetCore.Mvc;

namespace BioLife.MVC.Areas.Manage.Controllers
{
	[Area("Manage")]
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
