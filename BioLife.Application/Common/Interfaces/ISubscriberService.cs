

using BioLife.Domain.Entities;

namespace BioLife.Application.Common.Interfaces
{
	public interface ISubscriberService
	{
		Task<bool> SubscribeAsync(string email);
		Task<bool> UnsubscribeAsync(string email);
		Task<bool> IsSubscribedAsync(string email);
		Task<List<Subscriber>> GetAllSubscribersAsync();
		Task<bool> DeleteAsync(int id);
		Task SendEmailToAllAsync(string subject, string body);
	}
}
