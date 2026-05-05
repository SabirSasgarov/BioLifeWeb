using BioLife.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BioLife.Application.Common.Interfaces
{
	public interface IBasketService
	{
		Task<Basket> GetOrCreateBasketAsync(string userId);
		Task AddItemAsync(string userId, int productId, decimal quantity);
		Task RemoveItemAsync(string userId, int productId);
		Task UpdateQuantityAsync(string userId, int productId, decimal quantity);
		Task ClearBasketAsync(string userId);
		Task<int> GetBasketCountAsync(string userId);
	}
}
