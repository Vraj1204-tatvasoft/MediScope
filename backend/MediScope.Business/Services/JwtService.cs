using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MediScope.Common.Models;
using MediScope.Common.Models.Entities;

namespace MediScope.Business.Services
{
    public class JwtService : Interfaces.IJwtService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        /// <summary>
        /// Generates a signed JWT access token containing user claims.
        /// </summary>
        public string GenerateAccessToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Claims embedded inside the token — readable without DB call
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier,     user.Id.ToString()),
                new Claim(ClaimTypes.Email,              user.Email),
                new Claim(ClaimTypes.Name,               user.FullName),
                new Claim(ClaimTypes.Role,               user.Role.ToString()),
                new Claim("SessionId", user.CurrentSessionId.ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Generates a cryptographically random refresh token string.
        /// </summary>
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        /// <summary>
        /// Validates an expired access token and returns its claims.
        /// Used during token refresh — we trust the claims even if expired.
        /// </summary>
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,  // ← allow expired tokens here
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                                               Encoding.UTF8.GetBytes(_jwtSettings.SecretKey))
            };

            var handler = new JwtSecurityTokenHandler();

            try
            {
                var principal = handler.ValidateToken(token, tokenValidationParams, out var securityToken);

                if (securityToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}