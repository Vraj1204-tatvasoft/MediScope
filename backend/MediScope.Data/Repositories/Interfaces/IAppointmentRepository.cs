using MediScope.Common.Models.Entities;
using MediScope.Common.Models.DTOs.Response;
namespace MediScope.Data.Repositories
{
    public interface IAppointmentRepository
    {
        Task<Guid> CreateAppointmentViaSqlAsync(Guid doctorId, Guid patientId, DateTime startTime, int durationMinutes, string? doctorNotes, Guid createdBy);
        Task<bool> RespondToAppointmentViaSqlAsync(Guid appointmentId, Guid patientId, string action, string? patientNotes, Guid updatedBy);
        Task<List<DoctorSlotResponseDto>> GetDoctorScheduleAsync(Guid doctorId);
        Task<List<PatientAppointmentResponseDto>> GetPatientAppointmentsAsync(Guid patientId);
        Task<bool> RescheduleAppointmentViaSqlAsync(Guid appointmentId, Guid patientId, DateTime newStartTime, string? rescheduleReason, Guid updatedBy);
    }
}