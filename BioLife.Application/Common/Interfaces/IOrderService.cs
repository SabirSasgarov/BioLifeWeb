using BioLife.Domain.Entities;
using BioLife.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BioLife.Application.Common.Interfaces
{
	public interface IOrderService
	{
		Task<Order> CreateOrderAsync(string userId, string firstName, string lastName, string email,
			string phoneNumber, string address, string city, string postalCode, string country, string? note);
	
		Task <IEnumerable<Order?>> GetAllOrdersAsync();

		Task<Order> GetOrderByIdAsync(int id);

		Task<IEnumerable<Order>> GetOrderByUserAsync(string userId);

		Task UpdateOrderStatus(int id, OrderStatus status);
	}
}
