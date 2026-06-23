using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediScope.Common.Models.Enums;
using MediScope.Business.Helpers;
using MediScope.Data;
using MediScope.Business.Services.Interfaces;

namespace MediScope.ReminderService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MediScope Reminder Service started at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                        var now = DateTime.UtcNow;
                        var targetTime = now.AddMinutes(15);

                        var upcomingAppointments = await dbContext.Appointments
                            .Include(a => a.Doctor)
                                .ThenInclude(d => d.User)
                            .Include(a => a.Patient)
                                .ThenInclude(p => p.User)
                            .Where(a =>
                                a.Status == AppointmentStatus.Accepted &&
                                a.ReminderSent == false &&
                                a.StartTime <= targetTime &&
                                a.StartTime > now)
                            .ToListAsync(stoppingToken);

                        string frontendUrl = "http://localhost:4200";

                        foreach (var appt in upcomingAppointments)
                        {
                            var timeSpan = appt.StartTime - now;
                            int minutesRemaining = (int)timeSpan.TotalMinutes;

                            string timeText = minutesRemaining switch
                            {
                                >= 14 => "in 15 minutes",
                                > 1 => $"in {minutesRemaining} minutes",
                                _ => "shortly"
                            };

                            string patientHtml = EmailTemplates.AppointmentReminder(
                                recipientName: appt.Patient.User.FullName,
                                otherPartyName: appt.Doctor.User.FullName,
                                timeText: timeText,
                                isDoctor: false,
                                frontendUrl: frontendUrl
                            );

                            string doctorHtml = EmailTemplates.AppointmentReminder(
                                recipientName: appt.Doctor.User.FullName,
                                otherPartyName: $"{appt.Patient.User.FullName}",
                                timeText: timeText,
                                isDoctor: true,
                                frontendUrl: frontendUrl
                            );

                            string patientEmail = appt.Patient.User.Email;
                            string doctorEmail = appt.Doctor.User.Email;

                            try
                            {
                                await emailService.SendAsync(patientEmail, "Upcoming Appointment Reminder", patientHtml);
                                await emailService.SendAsync(doctorEmail, "Upcoming Appointment Reminder", doctorHtml);

                                _logger.LogInformation($"Sent HTML reminders for Appointment {appt.Id} starting {timeText}.");
                            }
                            catch (Exception emailEx)
                            {
                                _logger.LogWarning($"Email sent to SMTP server but timed out/errored for Appt {appt.Id}. Error: {emailEx.Message}");
                            }

                            appt.ReminderSent = true;
                        }

                        if (upcomingAppointments.Any())
                        {
                            await dbContext.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "A fatal error occurred while processing reminders.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}