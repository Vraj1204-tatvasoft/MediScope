using Microsoft.EntityFrameworkCore;

using MediScope.Common.Models.Entities;

namespace MediScope.Data.Repositories
{
    public class PatientRepository
        : GenericRepository<Patient>, IPatientRepository
    {
        public PatientRepository(
            AppDbContext context)
            : base(context)
        { }
        public async Task<IEnumerable<Patient>>
            GetAllAdminPatientsAsync()
        {
            return await _dbSet

                .Include(p => p.User)

                .Include(p => p.DoctorPatients)
                    .ThenInclude(dp => dp.Doctor)
                        .ThenInclude(d => d.User)

                .Include(p => p.HealthMetrics)
                    .ThenInclude(m => m.MetricDefinition)
                .Include(p => p.PatientAdmissions)
                .Where(p => !p.IsDeleted)

                .OrderByDescending(p => p.CreatedAt)

                .ToListAsync();
        }
        public async Task<Patient?> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(p => p.User)
                .FirstOrDefaultAsync(p =>
                    p.UserId == userId &&
                    !p.IsDeleted);
        }
    }
}