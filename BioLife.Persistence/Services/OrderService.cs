using BioLife.Domain.Entities;
using BioLife.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BioLife.Persistence.Services
{
	public class OrderService(
		AppDbContext appDbContext,
		IBasketService basketService) : IOrderService
	{
		public async Task<Order> CreateOrderAsync(string userId, string firstName, string lastName,
			string email, string phoneNumber, string address, string city, string postalCode,
			string country, string? note)
		{
			var basket = await appDbContext.Baskets
				.Include(b => b.BasketItems)
				.ThenInclude(bi => bi.Product)
				.FirstOrDefaultAsync(b => b.AppUserId == userId);

			if (basket == null || !basket.BasketItems.Any())
			{
				throw new InvalidOperationException("Basket is empty.");
			}
			var order = new Order
			{
				AppUserId = userId,
				FirstName = firstName,
				LastName = lastName,
				Email = email,
				Phone = phoneNumber,
				Address = address,
				City = city,
				PostalCode = postalCode,
				Country = country,
				Note = note,
				OrderStatus = OrderStatus.Pending,
				OrderItems = basket.BasketItems.Select(bi => new OrderItem
				{
					ProductId = bi.ProductId,
					ProductName = bi.Product.Name,
					UnitPrice = bi.Product.DiscountPrice ?? bi.Product.Price,
					Quantity = bi.Quantity
				}).ToList()
			};
			order.TotalAmount = order.OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity);

			appDbContext.Orders.Add(order);
			await appDbContext.SaveChangesAsync();
			await basketService.ClearBasketAsync(userId);
			return order;
		}

		public async Task<IEnumerable<Order>> GetAllOrdersAsync() => await appDbContext.Orders
			.Include(o => o.AppUser).Include(o => o.OrderItems)
			.ThenInclude(oi => oi.Product).Where(o => !o.IsDeleted)
			.OrderByDescending(o => o.CreatedDate).ToListAsync();

		public async Task<Order?> GetOrderByIdAsync(int id)=>
			await appDbContext.Orders.Include(o => o.AppUser)
			.Include(o => o.OrderItems)
			.ThenInclude(oi => oi.Product)
			.FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);


		public async Task<IEnumerable<Order>> GetOrderByUserAsync(string userId)=>
			await appDbContext.Orders.Include(o => o.AppUser)
			.Include(o => o.OrderItems)
			.ThenInclude(oi => oi.Product)
			.Where(o => o.AppUserId == userId && !o.IsDeleted)
			.OrderByDescending(o => o.CreatedDate).ToListAsync();

		public async Task UpdateOrderStatus(int id, OrderStatus status)
		{
			var order = await appDbContext.Orders.FindAsync(id);
			if (order == null)
			{
				throw new InvalidOperationException("Order not found.");
			}
			order.OrderStatus = status;
			await appDbContext.SaveChangesAsync();
		}
	}
}
