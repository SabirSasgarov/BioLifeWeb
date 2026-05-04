using Microsoft.AspNetCore.Mvc;

namespace BioLife.MVC.Areas.Manage.Controllers
{
	[Area("Manage")]
	public class UserController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
