namespace BioLife.MVC.Models
{
	public class CheckoutViewModel
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public string Address { get; set; }
		public string City { get; set; }
		public string PostalCode { get; set; }
		public string Country { get; set; }
		public string? Note { get; set; }
        public List<BasketItemViewModel> Items { get; set; } = [];
		public decimal subTotal => Items?.Sum(i => i.Price * i.Quantity) ?? 0;
		public decimal Total => subTotal;
	}
}
