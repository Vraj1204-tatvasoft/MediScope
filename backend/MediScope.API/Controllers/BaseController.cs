// File: MediScope.API/Controllers/BaseController.cs

using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MediScope.API.Controllers
{
    /// <summary>
    /// Base controller inherited by ALL feature controllers.
    /// Provides:
    ///   - Standard API response wrappers (Ok, Created, Error)
    ///   - CurrentUserId helper from JWT claims
    ///   - Consistent error handling pattern
    /// </summary>
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        // ── Current User ─────────────────────────────────────────────
        protected Guid CurrentUserId
        {
            get
            {
                var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? throw new UnauthorizedAccessException("User identity not found in token.");
                return Guid.Parse(claim);
            }
        }

        protected string CurrentUserRole
            => User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        // ── Success Responses ────────────────────────────────────────
        protected IActionResult Success<T>(T data, string message = "Success")
            => Ok(new ApiResponse<T>(data, message));

        protected IActionResult Created<T>(T data, string message = "Created successfully.")
            => StatusCode(201, new ApiResponse<T>(data, message));

        protected IActionResult NoContent(string message = "Operation completed.")
            => Ok(new ApiResponse(message, true));

        // ── Error Responses ──────────────────────────────────────────
        protected IActionResult NotFoundResponse(string message)
            => NotFound(new ApiResponse(message));

        protected IActionResult ConflictResponse(string message)
            => Conflict(new ApiResponse(message));

        protected IActionResult BadRequestResponse(string message)
            => BadRequest(new ApiResponse(message));

        protected IActionResult UnauthorizedResponse(string message)
            => Unauthorized(new ApiResponse(message));

        protected IActionResult ForbiddenResponse(string message)
            => StatusCode(403, new ApiResponse(message));

        protected IActionResult ServerError(string message)
            => StatusCode(500, new ApiResponse(message));
    }

    // ── API Response Wrappers ─────────────────────────────────────────
    // Kept here so BaseController has direct access.
    // Move to MediScope.Common if needed elsewhere.

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public ApiResponse(string message, bool success = false)
        {
            Message = message;
            Success = success;
        }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T Data { get; set; }

        public ApiResponse(T data, string message = "Success") : base(message, true)
        {
            Data = data;
        }
    }
}