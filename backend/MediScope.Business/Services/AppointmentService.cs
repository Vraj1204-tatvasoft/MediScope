using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Data.Repositories;

namespace MediScope.Business.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public AppointmentService(IUnitOfWork uow, ICurrentUserService currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<Guid> CreateAppointmentAsync(CreateAppointmentRequestDto request)
        {
            var doctor = await _uow.Doctors.GetByUserIdAsync(_currentUser.UserId)
                ?? throw new UnauthorizedAccessException("Only doctors can create appointments.");

            return await _uow.Appointments.CreateAppointmentViaSqlAsync(
                doctorId: doctor.Id,
                patientId: request.PatientId,
                startTime: request.StartTime,
                durationMinutes: request.DurationMinutes,
                doctorNotes: request.DoctorNotes,
                createdBy: _currentUser.UserId
            );
        }

        public async Task RespondToAppointmentAsync(RespondToAppointmentRequestDto request)
        {
            var patient = await _uow.Patients.GetByUserIdAsync(_currentUser.UserId)
                ?? throw new UnauthorizedAccessException("Only patients can respond to appointments.");

            if (request.Action.ToLower() == "rescheduled" && !request.RescheduledTo.HasValue)
            {
                throw new ArgumentException("A proposed rescheduled time is required when requesting a reschedule.");
            }

            await _uow.Appointments.RespondToAppointmentViaSqlAsync(
                appointmentId: request.AppointmentId,
                patientId: patient.Id,
                action: request.Action,
                patientNotes: request.PatientNotes,
                updatedBy: _currentUser.UserId
            );
        }

        public async Task<List<DoctorSlotResponseDto>> GetMyDoctorScheduleAsync()
        {
            var doctor = await _uow.Doctors.GetByUserIdAsync(_currentUser.UserId)
                ?? throw new UnauthorizedAccessException("Only doctors can access this schedule.");

            return await _uow.Appointments.GetDoctorScheduleAsync(doctor.Id);
        }

        public async Task<List<PatientAppointmentResponseDto>> GetMyPatientAppointmentsAsync()
        {
            var patient = await _uow.Patients.GetByUserIdAsync(_currentUser.UserId)
                ?? throw new UnauthorizedAccessException("Only patients can access these appointments.");

            return await _uow.Appointments.GetPatientAppointmentsAsync(patient.Id);
        }
        public async Task RescheduleAppointmentAsync(RespondToAppointmentRequestDto request)
        {
            var patient = await _uow.Patients.GetByUserIdAsync(_currentUser.UserId)
                ?? throw new UnauthorizedAccessException("Only patients can reschedule.");

            if (!request.RescheduledTo.HasValue)
            {
                throw new ArgumentException("A new date and time must be provided to reschedule.");
            }

            await _uow.Appointments.RescheduleAppointmentViaSqlAsync(
                appointmentId: request.AppointmentId,
                patientId: patient.Id,
                newStartTime: request.RescheduledTo.Value, // The new requested time
                rescheduleReason: request.RescheduleReason,
                updatedBy: _currentUser.UserId
            );
        }
    }
}