using BioLife.Application.Common.Interfaces;
using BioLife.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BioLife.Application.Services
{
	public class ProductService(IAppDbContext appDbContext) : IProductService
	{
		public async Task<List<Product>> GetAllAsync()
		{
			var products = await appDbContext.Products.OrderByDescending(p => p.Id).ToListAsync();
			return products;
		}

		public async Task<List<Product>> GetFeaturedAsync()
		{
			return await appDbContext.Products.Where(p => p.IsFeatured)
				.OrderByDescending(p => p.Id).Take(8).ToListAsync();
		}

		public async Task<Product?> GetByIdAsync(int id)
		{
			var product = await appDbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
			return product;
		}

		public async Task CreateAsync(Product product)
		{
			await appDbContext.Products.AddAsync(product);
			await appDbContext.SaveChangesAsync();
		}

		public async Task UpdateAsync(Product product)
		{
			appDbContext.Products.Update(product);
			await appDbContext.SaveChangesAsync();
		}

		public async Task DeleteAsync(int id)
		{
			var product = await appDbContext.Products.FindAsync(id);
			if (product != null)
			{
				product.IsDeleted = true;
				await appDbContext.SaveChangesAsync();
			}
		}

	}
}
