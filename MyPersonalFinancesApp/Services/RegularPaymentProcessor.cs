using FinanceManager.Data;
using FinanceManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FinanceManager.Services
{
    public class RegularPaymentProcessor : IHostedService, IDisposable
    {
        private readonly ILogger<RegularPaymentProcessor> _logger;
        private readonly IServiceProvider _services;
        private Timer? _timer;

        public RegularPaymentProcessor(ILogger<RegularPaymentProcessor> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Regular Payment Processor Service is starting.");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(1));
            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            _logger.LogInformation($"[{DateTime.Now}] Checking for due regular payments...");

            try
            {
                using (var scope = _services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    ProcessDuePayments(context).Wait();
                }
                _logger.LogInformation($"[{DateTime.Now}] Finished checking for payments.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing regular payments.");
            }
        }

        private async Task ProcessDuePayments(ApplicationDbContext context)
        {
            var now = DateTime.Now;
            var duePayments = await context.RegularPayments
                .Where(p => p.IsActive && p.NextRunDate <= now)
                .ToListAsync();

            if (!duePayments.Any())
            {
                return;
            }

            _logger.LogInformation($"Found {duePayments.Count} due payment(s) to process.");

            foreach (var payment in duePayments)
            {
                Transaction transaction = payment.Type == CategoryType.Income ? new Income() : new Expense();
                transaction.Amount = payment.Amount;
                transaction.Date = payment.NextRunDate;
                transaction.AccountId = payment.AccountId;
                transaction.CategoryId = payment.CategoryId;
                transaction.Comment = $"Automatic: {payment.Name}";

                context.Transactions.Add(transaction);
                _logger.LogInformation($"Created transaction for '{payment.Name}' for amount {payment.Amount}.");

                DateTime newNextRunDate = payment.NextRunDate;
                do
                {
                    switch (payment.FrequencyUnit)
                    {
                        case FrequencyUnit.Day: newNextRunDate = newNextRunDate.AddDays(payment.Interval); break;
                        case FrequencyUnit.Week: newNextRunDate = newNextRunDate.AddDays(payment.Interval * 7); break;
                        case FrequencyUnit.Month: newNextRunDate = newNextRunDate.AddMonths(payment.Interval); break;
                        case FrequencyUnit.Year: newNextRunDate = newNextRunDate.AddYears(payment.Interval); break;
                    }
                } while (newNextRunDate <= now);

                payment.NextRunDate = newNextRunDate;

                if (payment.EndDate.HasValue && payment.NextRunDate > payment.EndDate.Value)
                {
                    payment.IsActive = false;
                    _logger.LogInformation($"Deactivated payment '{payment.Name}' as its end date has passed.");
                }
            }

            await context.SaveChangesAsync();
            _logger.LogInformation("Finished processing and saving changes.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Regular Payment Processor Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}