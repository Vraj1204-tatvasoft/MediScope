using MediScope.Common.Models.Entities;

namespace MediScope.Data.Repositories
{
    public interface IPasswordResetTokenRepository : IGenericRepository<PasswordResetToken>
    {
        Task<PasswordResetToken?> GetValidTokenAsync(string token);
        Task InvalidateAllForUserAsync(Guid userId);
    }
}