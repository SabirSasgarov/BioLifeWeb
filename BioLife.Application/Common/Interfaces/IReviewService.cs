using BioLife.Domain.Entities;

namespace BioLife.Application.Common.Interfaces
{
	public interface IReviewService
	{
		Task<List<Review>> GetByProductIdAsync(int productId);
		Task AddAsync(Review review);
		Task<List<Review>> GetAllAsync(string? search, int page, int pageSize);
		Task<int> GetCountAsync(string? search);
		Task DeleteAsync(int reviewId);
	}
}
