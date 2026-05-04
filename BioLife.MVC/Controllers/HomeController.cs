using BioLife.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BioLife.MVC.Controllers
{
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Error404()
		{
			return View();
		}
	}
}
