using BioLife.Domain.Common;

namespace BioLife.Domain.Entities
{
	public class Subscriber : BaseEntity
	{
		public string Email { get; set; }
		public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
		public bool IsActive { get; set; } = true;
	}
}
