using Microsoft.AspNetCore.Mvc;

namespace BioLife.MVC.Controllers
{
	public class ContactController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
