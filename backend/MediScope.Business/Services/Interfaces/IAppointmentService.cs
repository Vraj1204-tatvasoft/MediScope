using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;

namespace MediScope.Business.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<Guid> CreateAppointmentAsync(CreateAppointmentRequestDto request);
        Task RespondToAppointmentAsync(RespondToAppointmentRequestDto request);
        Task<List<DoctorAppointmentResponseDto>> GetMyDoctorScheduleAsync();
        Task<List<PatientAppointmentResponseDto>> GetMyPatientAppointmentsAsync();
        Task RescheduleAppointmentAsync(RescheduleAppointmentRequestDto request);
        Task CancelAppointmentAsync(Guid appointmentId, string? reason);
        Task CompleteAppointmentAsync(Guid appointmentId);
        Task<List<AppointmentSummaryDto>> GetAppointmentsByPatientForDoctorAsync(Guid patientId);
    }
}