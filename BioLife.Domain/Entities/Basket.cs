
using BioLife.Domain.Common;
using BioLife.Domain.Entity;

namespace BioLife.Domain.Entities
{
	public class Basket : BaseEntity
	{
		public string AppUserId { get; set; }
		public AppUser AppUser { get; set; }
		public ICollection<BasketItem> BasketItems { get; set; } = new List<BasketItem>();
	}
}
