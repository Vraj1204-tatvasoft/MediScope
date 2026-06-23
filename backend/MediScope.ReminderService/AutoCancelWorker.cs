using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediScope.Common.Models.Enums;
using MediScope.Data;

namespace MediScope.ReminderService
{
    public class AutoCancelWorker : BackgroundService
    {
        private readonly ILogger<AutoCancelWorker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public AutoCancelWorker(ILogger<AutoCancelWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MediScope Auto-Cancel Worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        var cutoffTime = DateTime.UtcNow.AddHours(-24);

                        var expiredAppointments = await dbContext.Appointments
                            .Where(a =>
                                a.Status == AppointmentStatus.Accepted &&
                                a.EndTime <= cutoffTime)
                            .ToListAsync(stoppingToken);

                        foreach (var appt in expiredAppointments)
                        {
                            appt.Status = AppointmentStatus.Cancelled;

                            _logger.LogInformation($"Auto-cancelled Appointment {appt.Id} because it was pending for > 1 day.");
                        }

                        if (expiredAppointments.Any())
                        {
                            await dbContext.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while auto-cancelling appointments.");
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}