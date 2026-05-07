using BioLife.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BioLife.Persistence.Configurations
{
	public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
	{
		public void Configure(EntityTypeBuilder<OrderItem> builder)
		{
			builder.HasKey(builder => builder.Id);
			builder.Property(builder => builder.Quantity)
				.IsRequired()
				.HasColumnType("decimal(18,2)");
			builder.Property(builder => builder.UnitPrice)
				.IsRequired()
				.HasColumnType("decimal(18,2)");
			builder.Property(builder => builder.ProductName)
				.IsRequired()
				.HasMaxLength(200);	
			builder.HasOne(builder => builder.Product)
				.WithMany()
				.HasForeignKey(builder => builder.ProductId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
