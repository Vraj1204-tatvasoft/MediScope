using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MediScope.Data;
using MediScope.Business.Services;
using MediScope.Business.Services.Interfaces;
using MediScope.Business.Helpers;

Console.WriteLine($"[ {DateTime.Now} ] MediScope Background Scheduler Engine Active.");

// 1. SETUP HOST, CONFIGURATION, AND DEPENDENCY INJECTION
var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory());
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(context.Configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IEmailService, EmailService>();
    })
    .Build();

// 2. CREATE SCOPE TO EXECUTE TRANSACTION WORK
using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    var thresholdTime = DateTime.UtcNow.AddHours(-24);
    var minValidDate = new DateTime(2000, 1, 1).ToUniversalTime();

    try
    {
        // 3. FETCH ROWS & DEEP INCLUDE BASE USER TABLES ──
        var pendingRequests = await dbContext.DoctorPatients
            .Include(dp => dp.Doctor).ThenInclude(d => d.User) // Loads doctor's user account info
            .Include(dp => dp.Patient).ThenInclude(p => p.User) // Loads patient's user account info
            .Where(dp =>
                dp.Status == "pending_doctor" &&
                dp.AdminReviewedAt != null &&
                dp.AdminReviewedAt > minValidDate &&
                (
                    (dp.LastReminderSentAt == null && dp.AdminReviewedAt <= thresholdTime)
                    ||
                    (dp.LastReminderSentAt != null && dp.LastReminderSentAt <= thresholdTime)
                )
            )
            .ToListAsync();

        if (!pendingRequests.Any())
        {
            Console.WriteLine($"[ {DateTime.Now} ] Scan complete. No pending reminders required.");
            return;
        }

        Console.WriteLine($"[ {DateTime.Now} ] Found {pendingRequests.Count} requests needing attention. Processing...");

        var frontendUrl = config["App:FrontendUrl"] ?? "http://localhost:4200";

        foreach (var request in pendingRequests)
        {
            try
            {
                // Verify that data bindings are complete
                if (request.Doctor?.User == null || request.Patient?.User == null)
                {
                    Console.WriteLine($"[ WARN ] Skipping ID {request.Id}: User profile mappings are missing.");
                    continue;
                }

                var doctorName = request.Doctor.User.FullName;
                var doctorEmail = request.Doctor.User.Email;
                var patientName = request.Patient.User.FullName;

                var subject = "Action Required: Pending Patient Assignment";

                // Invoke our new template method cleanly
                var emailBody = EmailTemplates.PendingRequestReminder(doctorName, patientName, frontendUrl);

                await emailService.SendAsync(doctorEmail, subject, emailBody);

                request.LastReminderSentAt = DateTime.UtcNow;

                Console.WriteLine($"[ SUCCESS ] Reminder dispatched to Dr. {doctorName} ({doctorEmail})");
            }
            catch (Exception emailEx)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ FAILURE ] Problem processing entry link record: {emailEx.Message}");
                Console.ResetColor();
            }
        }

        await dbContext.SaveChangesAsync();
        Console.WriteLine($"[ {DateTime.Now} ] Database records updated successfully.");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ CRITICAL ERROR ] Process failed: {ex.Message}");
        Console.ResetColor();
    }
}

Console.WriteLine($"[ {DateTime.Now} ] Task finished. Exiting process safely.");