using Microsoft.EntityFrameworkCore;
using MediScope.Common.Models.Entities;

namespace MediScope.Data.Repositories
{
    public class PasswordResetTokenRepository
        : GenericRepository<PasswordResetToken>, IPasswordResetTokenRepository
    {
        public PasswordResetTokenRepository(AppDbContext context) : base(context) { }

        public async Task<PasswordResetToken?> GetValidTokenAsync(string token)
            => await _dbSet
                .Include(t => t.User)
                .FirstOrDefaultAsync(t =>
                    t.Token == token &&
                    !t.IsUsed &&
                    t.ExpiresAt > DateTime.UtcNow &&
                    !t.IsDeleted);

        public async Task InvalidateAllForUserAsync(Guid userId)
        {
            var existing = await _dbSet
                .Where(t => t.UserId == userId && !t.IsUsed && !t.IsDeleted)
                .ToListAsync();

            foreach (var t in existing)
            {
                t.IsUsed = true;
                t.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}