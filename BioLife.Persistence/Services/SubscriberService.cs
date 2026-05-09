using BioLife.Domain.Entities;

namespace BioLife.Persistence.Services
{
	public class SubscriberService(AppDbContext appDbContext,
		IEmailService emailService) : ISubscriberService
	{
		public async Task<bool> DeleteAsync(int id)
		{
			var subscriber = await appDbContext.Subscribers.FindAsync(id);
			if(subscriber == null)
				return false;
			appDbContext.Subscribers.Remove(subscriber);
			await appDbContext.SaveChangesAsync();
			return true;
		}

		public Task<List<Subscriber>> GetAllSubscribersAsync() => appDbContext.Subscribers
			.Where(s => !s.IsDeleted).OrderByDescending(s => s.SubscribedAt)
			.ToListAsync();

		public async Task<bool> IsSubscribedAsync(string email) => await appDbContext.Subscribers
				.AnyAsync(s => s.Email == email && !s.IsDeleted && s.IsActive);
		
		public async Task SendEmailToAllAsync(string subject, string body)
		{
			var subscribers = await appDbContext.Subscribers
				.Where(s => s.IsActive && !s.IsDeleted).ToListAsync();
			foreach (var subscriber in subscribers)
			{
				await emailService.SendEmailAsync(subscriber.Email, subject, body);
			}
		}

		public async Task<bool> SubscribeAsync(string email)
		{
			var existingSubscriber = await appDbContext.Subscribers
				.FirstOrDefaultAsync(s => s.Email == email && !s.IsDeleted);
			if (existingSubscriber != null)
			{
				existingSubscriber.IsActive = true;
			}
			else
			{
				appDbContext.Subscribers.Add(new Subscriber
				{
					Email = email,
				});
			}
			await appDbContext.SaveChangesAsync();
			return true;
		}

		public async Task<bool> UnsubscribeAsync(string email)
		{
			var subscriber = await appDbContext.Subscribers
				.FirstOrDefaultAsync(s => s.Email == email && !s.IsDeleted);
			
			if(subscriber == null)
				return false;
			
			subscriber.IsActive = false;
			await appDbContext.SaveChangesAsync();
			return true;
		}
	}
}
