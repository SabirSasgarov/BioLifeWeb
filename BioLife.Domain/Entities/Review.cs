using BioLife.Domain.Common;
using BioLife.Domain.Entity;

namespace BioLife.Domain.Entities
{
	public class Review : AuditableEntity
	{
		public string UserId { get; set; }
		public AppUser AppUser { get; set; }
		public int ProductId { get; set; }
		public Product Product { get; set; }
		public decimal Rating { get; set; }
		public string Comment { get; set; }
	}
}
