using MediScope.Business.Helpers;
using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.Entities;
using MediScope.Data.Repositories;
using Microsoft.Extensions.Configuration;

namespace MediScope.Business.Services
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly IUnitOfWork _uow;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        public PasswordResetService(
            IUnitOfWork uow,
            IEmailService emailService,
            IConfiguration config)
        {
            _uow = uow;
            _emailService = emailService;
            _config = config;
        }

        // FORGOT PASSWORD
        public async Task ForgotPasswordAsync(string email)
        {
            // Find user — silently return if not found (no user enumeration)
            var user = await _uow.Users
                .GetFirstOrDefaultAsync(u =>
                    u.Email == email.ToLower() && !u.IsDeleted);

            if (user == null) return;   // ← no error — prevents email enumeration

            // Invalidate existing tokens for this user
            await _uow.PasswordResetTokens.InvalidateAllForUserAsync(user.Id);

            // Generate cryptographically secure token
            var rawToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                                .Replace("+", "-")
                                .Replace("/", "_")
                                .Replace("=", "");

            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                Token = rawToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),  // 15 min expiry
                IsUsed = false,
                CreatedBy = user.Id,
                UpdatedBy = user.Id,
            };

            await _uow.PasswordResetTokens.AddAsync(resetToken);
            await _uow.SaveChangesAsync();

            // Build reset link — frontend URL
            var frontendUrl = _config["App:FrontendUrl"] ?? "http://localhost:4200";
            var resetLink = $"{frontendUrl}/reset-password?token={rawToken}";

            // Send email
            try
            {
                await _emailService.SendAsync(
                    to: email,
                    subject: "MediScope — Reset Your Password",
                    body: EmailTemplates.PasswordReset(
                                 user.FullName,
                                 resetLink,
                                 expiryMinutes: 15));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailService] Reset email failed: {ex.Message}");
            }
        }

        // VALIDATE TOKEN
        public async Task<bool> ValidateTokenAsync(string token)
        {
            var resetToken = await _uow.PasswordResetTokens
                .GetValidTokenAsync(token);

            return resetToken != null;
        }

        // RESET PASSWORD
        public async Task ResetPasswordAsync(string token, string newPassword)
        {
            var resetToken = await _uow.PasswordResetTokens
                .GetValidTokenAsync(token)
                ?? throw new InvalidOperationException(
                    "This reset link is invalid or has expired. Please request a new one.");

            var user = resetToken.User;

            // Prevent reusing same password
            if (BCrypt.Net.BCrypt.Verify(newPassword, user.PasswordHash))
                throw new InvalidOperationException("New password must be different from your current password.");

            // Update password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = user.Id;
            _uow.Users.Update(user);

            // Mark token as used
            _uow.PasswordResetTokens.Remove(resetToken);

            // Revoke all refresh tokens — force re-login on all devices
            var refreshTokens = await _uow.RefreshTokens
                .FindAsync(rt => rt.UserId == user.Id && !rt.IsRevoked);

            foreach (var rt in refreshTokens)
            {
                rt.IsRevoked = true;
                rt.RevokedAt = DateTime.UtcNow;
                _uow.RefreshTokens.Update(rt);
            }

            await _uow.SaveChangesAsync();
        }
    }
}