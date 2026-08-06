namespace MediScope.Business.Services.Interfaces
{
    public interface IPushService
    {
        Task<(bool Success, string? Error)> SendAsync(Guid userId, string title, string body, CancellationToken ct = default);
    }
}