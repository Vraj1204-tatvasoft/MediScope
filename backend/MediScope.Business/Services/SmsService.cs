using MediScope.Business.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace MediScope.Business.Services
{
    public class SmsService : ISmsService
    {
        private readonly ILogger<SmsService> _logger;

        public SmsService(ILogger<SmsService> logger)
        {
            _logger = logger;
        }

        public async Task<(bool Success, string? Error)> SendAsync(string toPhone, string message, CancellationToken ct = default)
        {
            try
            {
                await Task.Delay(15, ct);
                _logger.LogDebug("SMS → {Phone}", toPhone);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMS failed → {Phone}", toPhone);
                return (false, ex.Message);
            }
        }
    }
}