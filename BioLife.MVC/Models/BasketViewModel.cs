
namespace BioLife.MVC.Models
{
	public class BasketViewModel
	{
		public List<BasketItemViewModel> Items { get; set; } = [];

		public decimal Total => Items.Sum(i => i.SubTotal);
	}
}
