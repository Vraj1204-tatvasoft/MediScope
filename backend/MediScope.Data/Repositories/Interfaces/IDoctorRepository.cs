using MediScope.Common.Models.Entities;

namespace MediScope.Data.Repositories
{
    public interface IDoctorRepository : IGenericRepository<Doctor>
    {
        /// <summary>Get doctor by userId (from JWT) with User included</summary>
        Task<Doctor?> GetByUserIdAsync(Guid userId);

        /// <summary>Get doctor by id with User + DoctorPatients included</summary>
        Task<Doctor?> GetByIdWithDetailsAsync(Guid doctorId);

        /// <summary>Get all doctors with User included — for admin list</summary>
        Task<IEnumerable<Doctor>> GetAllWithUserAsync();

        /// <summary>Count of active patients assigned to this doctor</summary>
        Task<int> GetAssignedPatientCountAsync(Guid doctorId);
    }
}