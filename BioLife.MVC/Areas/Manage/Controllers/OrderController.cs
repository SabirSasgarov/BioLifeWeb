using BioLife.Application.Common.Interfaces;
using BioLife.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BioLife.MVC.Areas.Manage.Controllers
{
	[Area("Manage")]
	[Authorize(Roles = "Admin")]
	public class OrderController(IOrderService orderService) : Controller
	{
		private readonly IOrderService _orderService = orderService;
		public async Task<IActionResult> Index()
		{
			var orders = await _orderService.GetAllOrdersAsync();
			return View(orders);
		}
		public async Task<IActionResult> Details(int id)
		{
			var order = await _orderService.GetOrderByIdAsync(id);
			if (order == null)
				return NotFound();
			return View(order);
		}
		[HttpPost]
		[ValidateAntiForgeryToken] 
		public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
		{
			await _orderService.UpdateOrderStatus(id, status);
			return RedirectToAction(nameof(Details), new { id });
		}
	}
}
