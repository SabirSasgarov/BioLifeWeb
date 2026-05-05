using BioLife.Domain.Common;

namespace BioLife.Domain.Entities
{
	public class BasketItem : BaseEntity
	{
		public int ProductId { get; set; }
		public Product Product { get; set; }
		public int BasketId { get; set; }
		public Basket Basket { get; set; }
		public decimal Quantity { get; set; }
	}
}
