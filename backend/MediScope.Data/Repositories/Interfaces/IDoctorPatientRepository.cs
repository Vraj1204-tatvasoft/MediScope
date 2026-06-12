using MediScope.Common.Models.Entities;

namespace MediScope.Data.Repositories
{
    public interface IDoctorPatientRepository : IGenericRepository<DoctorPatient>
    {
        Task<DoctorPatient?> GetExistingLinkAsync(Guid? doctorId, Guid patientId);

        Task<DoctorPatient?> GetByIdWithDetailsAsync(Guid id);
        Task<IEnumerable<DoctorPatient>> GetByPatientIdAsync(Guid patientId);

        Task<IEnumerable<DoctorPatient>> GetByDoctorIdAsync(Guid doctorId);
        Task<IEnumerable<DoctorPatient>> GetPendingAdminRequestsAsync();
        Task<IEnumerable<DoctorPatient>> GetAllForAdminAsync();
        Task<IEnumerable<DoctorPatient>> GetPendingByDoctorIdAsync(Guid doctorId);
        IQueryable<DoctorPatient> GetAllWithDetailsQueryable();
    }
}