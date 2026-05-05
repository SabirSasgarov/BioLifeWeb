namespace BioLife.MVC.Models
{
	public record TwoFactorCacheEntry
	{
		public int? Attempts { get; set; }
		public string CodeHash { get; set; }
	}
}
