using BioLife.Application.Common.Interfaces;
using BioLife.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BioLife.MVC.Controllers
{
	[Authorize]
	public class CheckoutController(
		IBasketService basketService,
		IOrderService orderService) : Controller
	{
		private readonly IBasketService _basketService = basketService;
		private readonly IOrderService _orderService = orderService;
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var basket = await _basketService.GetOrCreateBasketAsync(userId);

			if (!basket.BasketItems.Any())
				return RedirectToAction("Index", "Basket");

			var vm = new CheckoutViewModel
			{
				Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
				Items = basket.BasketItems.Select(bi => new BasketItemViewModel
				{
					ProductId = bi.ProductId,
					ProductName = bi.Product.Name,
					Quantity = bi.Quantity,
					Price = bi.Product.DiscountPrice ?? bi.Product.Price,
					ImageUrl = bi.Product.ImageUrl
				}).ToList()
			};
			return View("Checkout", vm);
		}
		[HttpPost]
		public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (!ModelState.IsValid)
			{
				var basket = await _basketService.GetOrCreateBasketAsync(userId);
				model.Items = basket.BasketItems.Select(bi => new BasketItemViewModel
				{
					ProductId = bi.ProductId,
					ProductName = bi.Product.Name,
					Quantity = bi.Quantity,
					Price = bi.Product.DiscountPrice ?? bi.Product.Price,
					ImageUrl = bi.Product.ImageUrl
				}).ToList();
				return View("Checkout", model);
			}
			var order = await _orderService.CreateOrderAsync(userId, model.FirstName, model.LastName, model.Email,
				model.Phone, model.Address, model.City, model.PostalCode, model.Country, model.Note);

			Response.Cookies.Append("BasketCount", "0", new CookieOptions
			{
				Expires = DateTimeOffset.UtcNow.AddDays(7),
				HttpOnly = true,
				SameSite = SameSiteMode.Strict,
				Secure = true
			});


			return RedirectToAction("Confirmation", new { orderId = order.Id });
		}

		[HttpGet]
		public async Task<IActionResult> Confirmation(int orderId)
		{
			var user = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var order = await _orderService.GetOrderByIdAsync(orderId);
			if (order == null || order.AppUserId != user)
			{
				return NotFound();
			}
			var vm = new OrderConfirmationViewModel
			{
				OrderId = order.Id,
				FirstName = order.FirstName,
				LastName = order.LastName,
				Email = order.Email,
				Phone = order.Phone,
				Address = order.Address,
				City = order.City,
				PostalCode = order.PostalCode,
				Country = order.Country,
				Items = order.OrderItems.Select(oi => new OrderItemConfirmationViewModel
				{
					ProductName = oi.Product.Name,
					Quantity = oi.Quantity,
					UnitPrice = oi.UnitPrice
				}).ToList()
			};
			return View("OrderConfirmation", vm);
		}
	}
}
