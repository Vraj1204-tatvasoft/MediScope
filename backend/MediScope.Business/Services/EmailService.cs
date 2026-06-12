using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using MediScope.Business.Services.Interfaces;

namespace MediScope.Business.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            var host = _config["Email:Host"]
                        ?? throw new InvalidOperationException("Email:Host is not configured.");

            var port = int.Parse(_config["Email:Port"] ?? "587");
            var username = _config["Email:Username"]
                        ?? throw new InvalidOperationException("Email:Username is not configured.");
            var password = _config["Email:Password"]
                        ?? throw new InvalidOperationException("Email:Password is not configured.");
            var sender = _config["Email:SenderEmail"] ?? username;
            var senderName = _config["Email:SenderName"] ?? "MediScope";

            // ── Build SMTP client ─────────────────────────────────────
            using var smtp = new SmtpClient(host)
            {
                Port = port,
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Timeout = 10000,   // 10 seconds
            };

            // ── Build message ─────────────────────────────────────────
            using var mail = new MailMessage
            {
                From = new MailAddress(sender, senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mail.To.Add(to);

            // ── Send ──────────────────────────────────────────────────
            try
            {
                await smtp.SendMailAsync(mail);
            }
            catch (SmtpException ex)
            {
                throw new InvalidOperationException(
                    $"Email delivery failed — {ex.StatusCode}: {ex.Message}", ex);
            }
        }
    }
}