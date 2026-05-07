namespace BioLife.MVC.Models
{
	public class OrderConfirmationViewModel
	{
		public int OrderId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public string Address { get; set; }
		public string City { get; set; }
		public string PostalCode { get; set; }
		public string Country { get; set; }
		public decimal TotalAmount { get; set; }
		public string Phone { get; set; }
		public List<OrderItemConfirmationViewModel> Items { get; set; } = [];

	}

	public class OrderItemConfirmationViewModel
	{
		public string ProductName { get; set; }
		public decimal Quantity { get; set; }
		public decimal UnitPrice { get; set; }
		public decimal TotalPrice => UnitPrice * Quantity;
	}

}
