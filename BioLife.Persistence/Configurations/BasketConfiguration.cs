using BioLife.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BioLife.Persistence.Configurations
{
	public class BasketConfiguration : IEntityTypeConfiguration<Basket>
	{
		public void Configure(EntityTypeBuilder<Basket> builder)
		{
			builder.HasOne(b => b.AppUser)
				.WithOne()
				.HasForeignKey<Basket>(b => b.AppUserId);
		
			builder.HasQueryFilter(b=> !b.IsDeleted);
		}
		
	}
}
