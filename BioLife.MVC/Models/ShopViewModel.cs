using BioLife.Domain.Entities;
using BioLife.MVC.Helpers;

namespace BioLife.MVC.Models
{
	public class ShopViewModel
	{
		public PaginatedList<Product> Products { get; set; } = null!;
		public List<string> Categories { get; set; } = new();
		public string? Search { get; set; }
		public string? Category { get; set; }
		public string? Sort { get; set; }
		public int? PageSize { get; set; } = 8;
	}
}
