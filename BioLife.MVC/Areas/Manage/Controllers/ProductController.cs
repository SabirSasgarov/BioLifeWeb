using Microsoft.AspNetCore.Mvc;

namespace BioLife.MVC.Areas.Manage.Controllers
{
	[Area("Manage")]
	public class ProductController : Controller
	{
		public IActionResult Products()
		{
			return View();
		}
	}
}
