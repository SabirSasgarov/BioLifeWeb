using BioLife.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BioLife.Persistence.Configurations
{
	public class OrderConfiguration : IEntityTypeConfiguration<Order>
	{
		public void Configure(EntityTypeBuilder<Order> builder)
		{
			builder.HasKey(builder => builder.Id);
			builder.Property(builder => builder.FirstName) 
				.IsRequired()
				.HasMaxLength(50);
			builder.Property(builder => builder.LastName)
				.IsRequired()
				.HasMaxLength(50);
			builder.Property(builder => builder.Email)
				.IsRequired()
				.HasMaxLength(100);
			builder.Property(builder => builder.PostalCode)
				.IsRequired()
				.HasMaxLength(50);
			builder.Property(builder => builder.Country)
				.IsRequired()
				.HasMaxLength(50);
			builder.Property(builder => builder.City)
				.IsRequired()
				.HasMaxLength(50);
			builder.Property(builder => builder.Phone)
				.IsRequired()
				.HasMaxLength(50);

			builder.Property(builder => builder.TotalAmount)
				.IsRequired()
				.HasColumnType("decimal(18,2)");

			builder.Property(builder => builder.Address)
				.IsRequired()
				.HasMaxLength(200);


			builder.HasOne(builder => builder.AppUser)
				.WithMany()
				.HasForeignKey(builder => builder.AppUserId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasMany(builder => builder.OrderItems)
				.WithOne(builder => builder.Order)
				.HasForeignKey(builder => builder.OrderId);
		}
	}
}
