using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace MediScope.API.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, ex.Message);

                await WriteResponseAsync(
                    context,
                    HttpStatusCode.Forbidden,
                    ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, ex.Message);

                await WriteResponseAsync(
                    context,
                    HttpStatusCode.NotFound,
                    ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, ex.Message);

                await WriteResponseAsync(
                    context,
                    HttpStatusCode.BadRequest,
                    ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, ex.Message);

                await WriteResponseAsync(
                    context,
                    HttpStatusCode.Conflict,
                    ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unhandled exception occurred.");

                await WriteResponseAsync(
                    context,
                    HttpStatusCode.InternalServerError,
                    "An unexpected error occurred.");
            }
        }

        private static async Task WriteResponseAsync(
            HttpContext context,
            HttpStatusCode statusCode,
            string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                success = false,
                message = message
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }
}