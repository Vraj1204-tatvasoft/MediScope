// File: MediScope.Business/Services/Interfaces/IAdminDashboardService.cs

using MediScope.Common.Models.DTOs.Response;

namespace MediScope.Business.Services.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardResponseDto> GetDashboardAsync(Guid adminUserId);
    }
}