using BioLife.Domain.Entities;

namespace BioLife.Persistence.Services
{
	public class ReviewService(IAppDbContext appDbContext) : IReviewService
	{
		private IQueryable<Review> BuildQuery(string? search)
		{
			var query = appDbContext.Reviews.Where(r => !r.IsDeleted)
				.Include(r => r.AppUser)
				.Include(r => r.Product)
				.AsQueryable();
			if(!string.IsNullOrWhiteSpace(search))
			{
				query = query.Where(r => r.Comment.Contains(search) ||
				r.Product.Name.Contains(search)||
				r.AppUser.UserName!.Contains(search));
			}
			return query;
		}
		public async Task AddAsync(Review review)
		{
			await appDbContext.Reviews.AddAsync(review);
			await appDbContext.SaveChangesAsync();
		}

		public Task DeleteAsync(int reviewId)
		{
			var review = appDbContext.Reviews.Find(reviewId);
			if (review == null)
				throw new KeyNotFoundException($"Review with id {reviewId} not found.");
			appDbContext.Reviews.Remove(review);
			return appDbContext.SaveChangesAsync();
		}

		public async Task<List<Review>> GetAllAsync(string? search, int page, int pageSize) =>
			await BuildQuery(search)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

		public async Task<List<Review>> GetByProductIdAsync(int productId) => await appDbContext.Reviews
			.Where(r => r.ProductId == productId && !r.IsDeleted)
			.Include(r => r.AppUser)
			.OrderByDescending(r => r.CreatedDate)
			.ToListAsync();

		public Task<int> GetCountAsync(string? search) => BuildQuery(search).CountAsync();

	}
}
