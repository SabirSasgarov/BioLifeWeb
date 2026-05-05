using BioLife.Domain.Entities;

namespace BioLife.Application.Common.Interfaces
{
	public interface IProductService
	{
		Task CreateAsync(Product product);
		Task DeleteAsync(int id);
		Task<List<Product>> GetAllAsync();
		Task<Product?> GetByIdAsync(int id);
		Task<List<Product>> GetFeaturedAsync();
		Task UpdateAsync(Product product);
	}
}