// File: MediScope.API/Controllers/AuthController.cs
// Updated to extend BaseController

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Auth;
using MediScope.Common.Models.DTOs.Request;

namespace MediScope.API.Controllers
{
    [Route("api/auth")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly IPasswordResetService _passwordResetService;
        public AuthController(IAuthService authService, IPasswordResetService passwordResetService)
        {
            _authService = authService;
            _passwordResetService = passwordResetService;
        }

        // ── POST /api/auth/register ───────────────────────────────────
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequestResponse("Invalid registration data.");

            var response = await _authService.RegisterAsync(request);
            return Created(response, "Registration successful.");
        }

        // ── POST /api/auth/login ──────────────────────────────────────
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequestResponse("Invalid login data.");

            var response = await _authService.LoginAsync(request);
            return Success(response, "Login successful.");
        }

        // ── POST /api/auth/refresh ────────────────────────────────────
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequestResponse("Invalid request.");

            var response = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Success(response, "Token refreshed successfully.");
        }

        // ── POST /api/auth/logout ─────────────────────────────────────
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request)
        {
            await _authService.RevokeTokenAsync(request.RefreshToken);
            return NoContent("Logged out successfully.");
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequestResponse("Invalid email address.");

            await _passwordResetService.ForgotPasswordAsync(request.Email);

            // Always return same message — prevents email enumeration
            return Success(
                true,
                "If this email is registered, a reset link has been sent.");
        }

        [HttpGet("validate-reset-token")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateResetToken([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequestResponse("Token is required.");

            var isValid = await _passwordResetService.ValidateTokenAsync(token);

            if (!isValid)
                return BadRequestResponse("This reset link is invalid or has expired.");

            return Success(true, "Token is valid.");
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequestResponse("Invalid request data.");

            await _passwordResetService.ResetPasswordAsync(
                request.Token,
                request.NewPassword);

            return Success(
                true,
                "Password reset successfully. Please log in with your new password.");
        }
    }
}