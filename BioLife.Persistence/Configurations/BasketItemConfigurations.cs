using BioLife.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace BioLife.Persistence.Configurations
{
	public class BasketItemConfigurations : IEntityTypeConfiguration<BasketItem>
	{
		public void Configure(EntityTypeBuilder<BasketItem> builder)
		{
			builder.HasQueryFilter(b=> !b.IsDeleted);
		}

	}
}
