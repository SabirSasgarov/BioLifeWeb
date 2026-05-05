using BioLife.Domain.Entities;

namespace BioLife.Application.Common.Interfaces
{
	public interface IAppDbContext
	{
		public DbSet<Product> Products { get; }
		public DbSet<Basket> Baskets { get; }
		public DbSet<BasketItem> BasketItems { get; }
		Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}
