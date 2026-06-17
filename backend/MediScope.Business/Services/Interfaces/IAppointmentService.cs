using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Pagination;

namespace MediScope.Business.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<Guid> CreateAppointmentAsync(CreateAppointmentRequestDto request);
        Task RespondToAppointmentAsync(RespondToAppointmentRequestDto request);
        Task<List<DoctorSlotResponseDto>> GetMyDoctorScheduleAsync();
        Task<List<PatientAppointmentResponseDto>> GetMyPatientAppointmentsAsync();
        Task RescheduleAppointmentAsync(RespondToAppointmentRequestDto request);
    }
}