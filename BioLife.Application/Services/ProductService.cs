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
			return await appDbContext.Products.OrderByDescending(p => p.Id).ToListAsync(); 
		}

		public async Task<List<Product>> GetFeaturedAsync()
		{
			return await appDbContext.Products.Where(p => p.IsFeatured)
				.OrderByDescending(p => p.Id).Take(8).ToListAsync();
		}

		public async Task<Product?> GetByIdAsync(int id)
		{
			return await appDbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
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

		public IQueryable<Product> GetFilteredQuery(string? search, string? category, string? sort)
		{
			var query = appDbContext.Products.Where(p=>!p.IsDeleted).AsQueryable();
			if(!string.IsNullOrWhiteSpace(category) && category != "all")
			{
				query = query.Where(p => p.Category == category);
			}
			if(!string.IsNullOrWhiteSpace(search))
			{
				query = query.Where(p => p.Name.Contains(search) ||
				(p.Description != null && p.Description.Contains(search)));
			}
			query = sort switch
			{
				"price_asc" => query.OrderBy(p => p.DiscountPrice ?? p.Price),
				"price_desc" => query.OrderByDescending(p => p.DiscountPrice ?? p.Price),
				"name_asc" => query.OrderBy(p => p.Name),
				"name_desc" => query.OrderByDescending(p => p.Name),
				"newest" => query.OrderByDescending(p => p.CreatedDate),
				"feateured" => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p=>p.CreatedDate),
				_ => query.OrderByDescending(p => p.Id)
			};
			return query;
		}

		public async Task<List<string>> GetCategoriesAsync() =>
			await appDbContext.Products
			.Where(p => !p.IsDeleted && p.Category != null)
			.Select(p => p.Category!)
			.Distinct()
			.OrderBy(c => c)
			.ToListAsync();
		
	}
}
