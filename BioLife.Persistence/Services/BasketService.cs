using BioLife.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BioLife.Application.Services
{
	public class BasketService(AppDbContext appDbContext) : IBasketService
	{
		public async Task AddItemAsync(string userId, int productId, decimal quantity)
		{
			var basket = await GetOrCreateBasketAsync(userId);
			var item = basket.BasketItems.FirstOrDefault(i => i.ProductId == productId);
			if(item != null)
			{
				item.Quantity += quantity;
			}
			else
			{
				basket.BasketItems.Add(new BasketItem
				{
					ProductId = productId,
					Quantity = quantity
				});
			}

			await appDbContext.SaveChangesAsync();
		}

		public async Task ClearBasketAsync(string userId)
		{
			var basket = await GetOrCreateBasketAsync(userId);
			appDbContext.BasketItems.RemoveRange(basket.BasketItems);
			await appDbContext.SaveChangesAsync();
		}

		public async Task<int> GetBasketCountAsync(string userId)
		{
			var basket = await appDbContext.Baskets
				.Include(b => b.BasketItems)
				.FirstOrDefaultAsync(b => b.AppUserId == userId);
			return basket?.BasketItems.Count ?? 0;
		}

		public async Task<Basket> GetOrCreateBasketAsync(string userId)
		{
			var basket = appDbContext.Baskets
				.Include(b => b.BasketItems)
				.ThenInclude(bi => bi.Product)
				.FirstOrDefault(b => b.AppUserId == userId);
			if (basket == null)
			{
				basket = new Basket { AppUserId = userId };
				await appDbContext.Baskets.AddAsync(basket);
				await appDbContext.SaveChangesAsync();
			}
			return basket;
		}

		public async Task RemoveItemAsync(string userId, int productId)
		{
			var basket = await GetOrCreateBasketAsync(userId);
			var item = basket.BasketItems.FirstOrDefault(i => i.ProductId == productId);

			if (item!=null)
			{
				appDbContext.BasketItems.Remove(item);
				await appDbContext.SaveChangesAsync();
			}
		}

		public async Task UpdateQuantityAsync(string userId, int productId, decimal quantity)
		{
			var basket = await GetOrCreateBasketAsync(userId);
			var item = basket.BasketItems.FirstOrDefault(i => i.ProductId == productId);
			if (item != null)
			{
				if(quantity <= 0)
				{
					appDbContext.BasketItems.Remove(item);
				}
				else
					item.Quantity = quantity;
				await appDbContext.SaveChangesAsync();
			}
		}
	}
}

