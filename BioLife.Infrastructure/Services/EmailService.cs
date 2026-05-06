using System.Net;
using System.Net.Mail;

namespace BioLife.Infrastructure.Services
{
	public class EmailService(IConfiguration configuration) : IEmailService
	{
		private readonly IConfiguration _configuration = configuration;
		public async Task SendEmailAsync(string toEmail, string subject, string body)
		{
			var emailSettings = _configuration.GetSection("EmailSettings");
			var host = emailSettings["Host"];
			var port = int.Parse(emailSettings["Port"] ?? "587");
			var email = emailSettings["SenderEmail"];
			var password = emailSettings["Password"];

			using var client = new SmtpClient(host, port)
			{
				Credentials = new NetworkCredential(email, password),
				EnableSsl = true
			};

			var mailMessage = new MailMessage{
				From = new MailAddress(email),
				Subject = subject,
				Body = body,
				IsBodyHtml = true
			};

			mailMessage.To.Add(toEmail);
			await client.SendMailAsync(mailMessage);
		}
	}
}
