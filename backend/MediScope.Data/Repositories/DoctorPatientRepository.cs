using Microsoft.EntityFrameworkCore;
using MediScope.Common.Models.Entities;

namespace MediScope.Data.Repositories
{
    public class DoctorPatientRepository
        : GenericRepository<DoctorPatient>, IDoctorPatientRepository
    {
        public DoctorPatientRepository(AppDbContext context) : base(context) { }

        public async Task<DoctorPatient?> GetExistingLinkAsync(Guid? doctorId, Guid patientId)
        {
            var query = _dbSet.Where(dp =>
                dp.PatientId == patientId &&
                !dp.IsDeleted);

            if (doctorId.HasValue)
            {
                // Check same doctor-patient link
                query = query.Where(dp => dp.DoctorId == doctorId);
            }
            else
            {
                // Check only unassigned requests
                query = query.Where(dp => dp.DoctorId == null);
            }

            return await query.FirstOrDefaultAsync();
        }
        public async Task<DoctorPatient?> GetByIdWithDetailsAsync(Guid id)
            => await _dbSet
                .Include(dp => dp.Doctor).ThenInclude(d => d!.User)
                .Include(dp => dp.Patient).ThenInclude(p => p.User)
                .FirstOrDefaultAsync(dp => dp.Id == id && !dp.IsDeleted);

        public async Task<IEnumerable<DoctorPatient>> GetByPatientIdAsync(Guid patientId)
            => await _dbSet
                .Include(dp => dp.Doctor).ThenInclude(d => d!.User)
                .Where(dp => dp.PatientId == patientId && !dp.IsDeleted)
                .OrderByDescending(dp => dp.RequestedAt)
                .ToListAsync();

        public async Task<IEnumerable<DoctorPatient>> GetByDoctorIdAsync(Guid doctorId)
            => await _dbSet
                .Include(dp => dp.Patient).ThenInclude(p => p.User)
                .Where(dp => dp.DoctorId == doctorId && !dp.IsDeleted)
                .OrderByDescending(dp => dp.RequestedAt)
                .ToListAsync();

        public async Task<IEnumerable<DoctorPatient>> GetPendingByDoctorIdAsync(Guid doctorId)
            => await _dbSet
                .Include(dp => dp.Patient).ThenInclude(p => p.User)
                .Where(dp => dp.DoctorId == doctorId &&
                             dp.Status == "pending_doctor" &&
                             !dp.IsDeleted)
                .OrderByDescending(dp => dp.RequestedAt)
                .ToListAsync();

        public async Task<IEnumerable<DoctorPatient>> GetPendingAdminRequestsAsync()
            => await _dbSet
                .Include(dp => dp.Patient).ThenInclude(p => p.User)
                .Include(dp => dp.Doctor).ThenInclude(d => d!.User)
                .Where(dp => dp.Status == "pending_admin" && !dp.IsDeleted)
                .OrderByDescending(dp => dp.RequestedAt)
                .ToListAsync();

        public async Task<IEnumerable<DoctorPatient>> GetAllForAdminAsync()
            => await _dbSet
                .Include(dp => dp.Patient).ThenInclude(p => p.User)
                .Include(dp => dp.Doctor).ThenInclude(d => d!.User)
                .Where(dp => !dp.IsDeleted)
                .OrderByDescending(dp => dp.RequestedAt)
                .ToListAsync();

        public IQueryable<DoctorPatient> GetAllWithDetailsQueryable()
            => _dbSet
                .Include(dp => dp.Doctor).ThenInclude(d => d!.User)
                .Include(dp => dp.Patient).ThenInclude(p => p.User)
                .Where(dp => !dp.IsDeleted);
    }
}