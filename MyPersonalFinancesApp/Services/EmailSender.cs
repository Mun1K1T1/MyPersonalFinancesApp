using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace FinanceManager.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogWarning($"--- NEW EMAIL ---");
            _logger.LogWarning($"To: {email}");
            _logger.LogWarning($"Subject: {subject}");
            _logger.LogWarning($"Body (copy link from here): {htmlMessage}");
            _logger.LogWarning($"-----------------");

            return Task.CompletedTask;
        }
    }
}