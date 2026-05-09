
using BioLife.Domain.Entities;

namespace BioLife.Persistence.Contexts
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) :
		IdentityDbContext<AppUser, AppRole, string>(options), IAppDbContext
	{

		public DbSet<Product> Products { get; set; }
		public DbSet<Basket> Baskets { get; set; }
		public DbSet<BasketItem> BasketItems { get; set; }
		public DbSet<Order> Orders { get; set; }
		public DbSet<OrderItem> OrderItems { get; set; }
		public DbSet<Subscriber> Subscribers { get; set; }
		public DbSet<Review> Reviews { get; set; }

		public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			foreach (var entry in ChangeTracker.Entries<BaseEntity>())
			{
				if (entry.State == EntityState.Deleted)
				{
					entry.State = EntityState.Modified;
					entry.Entity.IsDeleted = true;

					if (entry.Entity is AuditableEntity auditableEntity)
					{
						auditableEntity.DeletedDate = DateTime.UtcNow;
						auditableEntity.DeletedBy = "System";
					}
				}
			}
			foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
			{
				switch (entry.State)
				{
					case EntityState.Added:
						entry.Entity.CreatedDate = DateTime.UtcNow;
						entry.Entity.CreatedBy = "System";
						break;
					case EntityState.Modified:
						entry.Entity.ModifiedDate = DateTime.UtcNow;
						entry.Entity.ModifiedBy = "System";
						break;
				}
			}
			return base.SaveChangesAsync(cancellationToken);
		}
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
		}
	}
}
