using Microsoft.EntityFrameworkCore;
using MediScope.Common.Models.Entities;

namespace MediScope.Data.Repositories
{
    public class PatientDashboardRepository : IPatientDashboardRepository
    {
        private readonly AppDbContext _context;

        public PatientDashboardRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Patient?> GetDashboardDataAsync(Guid userId)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .Include(p => p.DoctorPatients)
                    .ThenInclude(dp => dp.Doctor)
                        .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(p =>
                    p.UserId == userId &&
                    !p.IsDeleted);

            if (patient != null)
            {
                patient.HealthMetrics = await _context.HealthMetrics
                    .Include(m => m.RecordedByUser)
                    .Include(m => m.MetricDefinition)
                    .Where(m => m.PatientId == patient.Id && !m.IsDeleted)
                    .ToListAsync();
            }

            return patient;
        }
    }
}