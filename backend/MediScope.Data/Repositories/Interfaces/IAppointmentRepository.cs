using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Entities;

namespace MediScope.Data.Repositories
{
    public interface IAppointmentRepository
    {
        Task<Guid> CreateAppointmentViaSqlAsync(Guid doctorId, Guid patientId, DateTime startTime, int durationMinutes, string? doctorNotes, Guid createdBy);
        Task<bool> RespondToAppointmentViaSqlAsync(Guid appointmentId, string action, string? notes, Guid userId);
        Task<List<DoctorAppointmentResponseDto>> GetDoctorScheduleAsync(Guid doctorId);
        Task<List<PatientAppointmentResponseDto>> GetPatientAppointmentsAsync(Guid patientId);
        Task<bool> RequestRescheduleViaSqlAsync(Guid appointmentId, Guid userId, DateTime newStartTime, string? rescheduleReason);
        Task<bool> CancelAppointmentViaSqlAsync(Guid appointmentId, Guid actorId, string? cancelReason);
        Task<Appointment?> GetByIdAsync(Guid id);
        Task<List<Appointment>> GetAppointmentsByPatientAndDoctorAsync(Guid patientId, Guid doctorId);
    }
}