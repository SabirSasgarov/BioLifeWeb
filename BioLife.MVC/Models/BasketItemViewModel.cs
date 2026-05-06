namespace BioLife.MVC.Models
{
	public class BasketItemViewModel
	{
		public int ProductId { get; set; }
		public string ProductName { get; set; }
		public string? ImageUrl { get; set; }
		public decimal Quantity { get; set; }
		public decimal Price { get; set; }
		public decimal SubTotal=> Quantity * Price;
	}
}
