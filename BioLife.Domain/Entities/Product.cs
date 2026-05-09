using BioLife.Domain.Common;

namespace BioLife.Domain.Entities
{
	public class Product : AuditableEntity
	{
		public string Name { get; set; } = string.Empty;
		public string? ShortDescription { get; set; }
		public string? Description { get; set; }
		public string SKU { get; set; } = string.Empty;
		public decimal Price { get; set; }
		public decimal? DiscountPrice { get; set; }
		public int StockQuantity { get; set; }
		public string? ImageUrl { get; set; }
		public string? Category { get; set; }
		public int StarRating { get; set; }
		public bool IsFeatured { get; set; }
		public List<Review> Reviews { get; set; }
	}
}
