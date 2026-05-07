using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BioLife.MVC.Areas.Manage.Controllers
{
	[Area("Manage")]
	[Authorize(Roles = "Admin")]
	public class DashboardController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
