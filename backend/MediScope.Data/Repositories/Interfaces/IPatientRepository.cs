using MediScope.Common.Models.Entities;

namespace MediScope.Data.Repositories
{
    public interface IPatientRepository
        : IGenericRepository<Patient>
    {
        Task<IEnumerable<Patient>> GetAllAdminPatientsAsync();
        Task<Patient?> GetByUserIdAsync(Guid userId);
        Task<Patient?> GetPatientByIdAsync(Guid patientId);
    }
}