using Microsoft.EntityFrameworkCore;
using MediScope.Data;
using MediScope.Common;
using MediScope.Common.Models;
using MediScope.Common.Models.DTOs.Auth;
using MediScope.Common.Models.Entities;
using MediScope.Common.Models.Enums;
using MediScope.Business.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace MediScope.Business.Services
{
    public class AuthService : Interfaces.IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            AppDbContext context,
            IJwtService jwtService,
            IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _jwtService = jwtService;
            _jwtSettings = jwtSettings.Value;
        }

        // ── REGISTER ─────────────────────────────────────────────────
        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            // 1. Check email not already taken
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == request.Email.ToLower());

            if (emailExists)
                throw new InvalidOperationException("An account with this email already exists.");

            // 2. Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // 3. Create user — NO CreatedBy yet (causes self-ref FK issue)
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email.ToLower(),
                PasswordHash = passwordHash,
                Role = UserRole.Patient,
                IsActive = true
                // CreatedBy intentionally left null here
            };

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 4. Insert user first — row must exist before anything refs it
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                // 5. Now user.Id is confirmed in DB — set CreatedBy and update
                user.CreatedBy = user.Id;
                user.UpdatedBy = user.Id;
                await _context.SaveChangesAsync();

                // 6. Create patient — user row exists now, FK is safe
                var patient = new Patient
                {
                    UserId = user.Id,
                    DateOfBirth = request.DateOfBirth,
                    Gender = request.Gender,
                    BloodGroup = request.BloodGroup,
                    ContactNumber = request.ContactNumber,
                    Address = request.Address,
                    ConsentProfileVisible = false,
                    CreatedBy = user.Id,
                    UpdatedBy = user.Id
                };

                await _context.Patients.AddAsync(patient);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            // 7. Generate tokens
            return await GenerateAndSaveTokens(user);
        }

        // ── LOGIN ─────────────────────────────────────────────────────
        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

            if (user is null)
                throw new UnauthorizedAccessException("Invalid email or password.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Your account has been deactivated. Contact support.");

            var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!passwordValid)
                throw new UnauthorizedAccessException("Invalid email or password.");

            return await GenerateAndSaveTokens(user);
        }

        // ── REFRESH TOKEN ─────────────────────────────────────────────
        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken is null)
                throw new UnauthorizedAccessException("Invalid refresh token.");

            if (!storedToken.IsActive)
                throw new UnauthorizedAccessException("Refresh token has expired or been revoked.");

            // Revoke old token before issuing new one (rotation)
            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GenerateAndSaveTokens(storedToken.User);
        }

        // ── REVOKE TOKEN (Logout) ─────────────────────────────────────
        public async Task RevokeTokenAsync(string refreshToken)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken is null || !storedToken.IsActive)
                throw new UnauthorizedAccessException("Invalid or already revoked token.");

            _context.RefreshTokens.Remove(storedToken);

            await _context.SaveChangesAsync();
        }

        // ── PRIVATE HELPER ────────────────────────────────────────────
        private async Task<AuthResponseDto> GenerateAndSaveTokens(User user)
        {
            var existingTokens = await _context.RefreshTokens.Where(rt => rt.UserId == user.Id && !rt.IsRevoked).ToListAsync();

            var hadActiveSessions = existingTokens.Any();

            foreach (var rt in existingTokens)
            {
                rt.IsRevoked = true;
                rt.RevokedAt = DateTime.UtcNow;
                _context.RefreshTokens.Update(rt);
            }

            user.CurrentSessionId = Guid.NewGuid();
            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshTokenString = _jwtService.GenerateRefreshToken();
            var accessTokenExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes);

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenString,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                IsRevoked = false,
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenString,
                AccessTokenExpiry = accessTokenExpiry,
                MustChangePassword = user.MustChangePassword,
                User = new UserProfileDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role.ToString()
                }
            };
        }
    }
}