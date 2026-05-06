using BioLife.Application.Common.Interfaces;
using BioLife.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BioLife.MVC.Controllers
{
	[Authorize]
	public class BasketController(IBasketService basketService) : Controller
	{
		private readonly IBasketService _basketService = basketService;
		private void UpdateBasketCookie(decimal count)
		{
			Response.Cookies.Append("BasketCount", count.ToString(), new CookieOptions
			{
				Expires = DateTimeOffset.UtcNow.AddDays(30),
				HttpOnly = false,
				Secure = true,
				SameSite = SameSiteMode.Strict
			});

		}
		public async Task<IActionResult> Index()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId is null)
			{
				return RedirectToAction("Login", "Account");
			}
			var basket = await _basketService.GetOrCreateBasketAsync(userId);
			decimal count = basket.BasketItems.Sum(bi => bi.Quantity);
			UpdateBasketCookie(count);

			var vm = new BasketViewModel
			{
				Items = basket.BasketItems.Select(bi => new BasketItemViewModel
				{
					ProductName = bi.Product.Name,
					ProductId = bi.ProductId,
					Quantity = bi.Quantity,
					ImageUrl = bi.Product.ImageUrl,
					Price = bi.Product.DiscountPrice ?? bi.Product.Price
				}).ToList()
			};
			return View(vm);
		}
		[HttpPost]
		public async Task<IActionResult> Add(int productId, decimal quantity = 1)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId is null)
			{
				return RedirectToAction("Login", "Account");
			}
			await _basketService.AddItemAsync(userId, productId, quantity);

			decimal count = await _basketService.GetBasketCountAsync(userId);
			UpdateBasketCookie(count);

			if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				return Json(new { success = true, count });
			}
			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> Remove(int productId)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId is null)
			{
				return RedirectToAction("Login", "Account");
			}
			await _basketService.RemoveItemAsync(userId, productId);

			decimal count = await _basketService.GetBasketCountAsync(userId);
			UpdateBasketCookie(count);
			if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				return Json(new { success = true, count });
			}
			return RedirectToAction("Index");
		}
		[HttpPost]
		public async Task<IActionResult> UpdateQuantity(int productId, decimal quantity)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId is null)
			{
				return RedirectToAction("Login", "Account");
			}
			await _basketService.UpdateQuantityAsync(userId, productId, quantity);
			decimal count = await _basketService.GetBasketCountAsync(userId);
			UpdateBasketCookie(count);
			if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				var basket = await _basketService.GetOrCreateBasketAsync(userId);
				var item = basket.BasketItems.FirstOrDefault(bi => bi.ProductId == productId);
				var itemTotal = item != null ? (item.Product.DiscountPrice ?? item.Product.Price) * item.Quantity : 0;
				var cartTotal = basket.BasketItems.Sum(bi => (bi.Product.DiscountPrice ?? bi.Product.Price) * bi.Quantity);

				return Json(new
				{
					success = true,
					count,
					itemTotal = itemTotal.ToString("0.00"),
					cartTotal = cartTotal.ToString("0.00")
				});
			}
			return RedirectToAction("Index");
		}
		[HttpPost]
		public async Task<IActionResult> Clear()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId is null)
			{
				return RedirectToAction("Login", "Account");
			}
			await _basketService.ClearBasketAsync(userId);
			UpdateBasketCookie(0);
			if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				return Json(new { success = true, count = 0 });
			}
			return RedirectToAction("Index");

		}
	}
}
