namespace MediScope.Business.Services.Interfaces
{
    public interface ISmsService
    {
        Task<(bool Success, string? Error)> SendAsync(string toPhone, string message, CancellationToken ct = default);
    }
}