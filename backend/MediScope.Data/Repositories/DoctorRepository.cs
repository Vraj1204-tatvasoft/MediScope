using Microsoft.EntityFrameworkCore;
using MediScope.Common.Models.Entities;
using MediScope.Common.Models.Enums;

namespace MediScope.Data.Repositories
{
    public class DoctorRepository : GenericRepository<Doctor>, IDoctorRepository
    {
        public DoctorRepository(AppDbContext context) : base(context) { }

        public async Task<Doctor?> GetByUserIdAsync(Guid userId)
            => await _dbSet
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId && !d.IsDeleted);

        public async Task<Doctor?> GetByIdWithDetailsAsync(Guid doctorId)
            => await _dbSet
                .Include(d => d.User)
                .Include(d => d.DoctorPatients)
                .FirstOrDefaultAsync(d => d.Id == doctorId && !d.IsDeleted);

        public async Task<IEnumerable<Doctor>> GetAllWithUserAsync()
            => await _dbSet
                .Include(d => d.User)
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.User.FullName)
                .ToListAsync();

        public async Task<int> GetAssignedPatientCountAsync(Guid doctorId)
            => await _context.Set<DoctorPatient>()
                .CountAsync(dp => dp.DoctorId == doctorId
                               && dp.Status == ConnectionStatus.Active
                               && !dp.IsDeleted);
    }
}