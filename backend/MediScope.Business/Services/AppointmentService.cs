using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Data.Repositories;
using MediScope.Common.Models.Enums;

namespace MediScope.Business.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;
        private readonly INotificationService _notificationService;

        public AppointmentService(IUnitOfWork uow, ICurrentUserService currentUser, INotificationService notificationService)
        {
            _uow = uow;
            _currentUser = currentUser;
            _notificationService = notificationService;
        }

        public async Task<Guid> CreateAppointmentAsync(CreateAppointmentRequestDto request)
        {
            if (request.StartTime < DateTime.UtcNow)
            {
                throw new ArgumentException("Cannot book an appointment for a time that has already passed.");
            }
            var doctor = await _uow.Doctors.GetByUserIdAsync(_currentUser.UserId)
                ?? throw new UnauthorizedAccessException("Only doctors can create appointments.");

            var appointmentId = await _uow.Appointments.CreateAppointmentViaSqlAsync(
                doctorId: doctor.Id,
                patientId: request.PatientId,
                startTime: request.StartTime,
                durationMinutes: request.DurationMinutes,
                doctorNotes: request.DoctorNotes,
                createdBy: _currentUser.UserId
            );

            var patient = await _uow.Patients.GetByIdAsync(request.PatientId);
            if (patient != null)
            {
                await _notificationService.CreateAsync(
                    patient.UserId,
                    NotificationType.Info,
                    "A new appointment has been scheduled for you. Please review the details."
                );
            }

            return appointmentId;
        }

        public async Task RespondToAppointmentAsync(RespondToAppointmentRequestDto request)
        {
            var doctor = await _uow.Doctors.GetByUserIdAsync(_currentUser.UserId);
            var patient = await _uow.Patients.GetByUserIdAsync(_currentUser.UserId);

            var actorProfileId = doctor?.Id ?? patient?.Id
                ?? throw new UnauthorizedAccessException("You must have a doctor or patient profile to respond to appointments.");

            await _uow.Appointments.RespondToAppointmentViaSqlAsync(
                appointmentId: request.AppointmentId,
                action: request.Action,
                notes: request.PatientNotes,
                userId: actorProfileId
            );

            var targetUserId = await GetOtherPartyUserIdAsync(request.AppointmentId);
            await _notificationService.CreateAsync(
                targetUserId,
                NotificationType.Info,
                $"Your appointment request was {request.Action.ToLower()}."
            );
        }

        public async Task<List<DoctorAppointmentResponseDto>> GetMyDoctorScheduleAsync()
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

        public async Task RescheduleAppointmentAsync(RescheduleAppointmentRequestDto request)
        {
            if (request.RescheduledTo < DateTime.UtcNow)
            {
                throw new ArgumentException("Cannot reschedule an appointment to a time that has already passed.");
            }
            await _uow.Appointments.RequestRescheduleViaSqlAsync(
                appointmentId: request.AppointmentId,
                userId: _currentUser.UserId,
                newStartTime: request.RescheduledTo,
                rescheduleReason: request.RescheduleReason
            );
            var targetUserId = await GetOtherPartyUserIdAsync(request.AppointmentId);
            await _notificationService.CreateAsync(
                targetUserId,
                NotificationType.Info,
                "The other party has requested to reschedule an upcoming appointment."
            );
        }
        public async Task CancelAppointmentAsync(Guid appointmentId, string? reason)
        {
            var doctor = await _uow.Doctors.GetByUserIdAsync(_currentUser.UserId);
            var patient = await _uow.Patients.GetByUserIdAsync(_currentUser.UserId);

            var actorProfileId = doctor?.Id ?? patient?.Id
                ?? throw new UnauthorizedAccessException("You must have a doctor or patient profile to cancel an appointment.");

            await _uow.Appointments.CancelAppointmentViaSqlAsync(
                appointmentId: appointmentId,
                actorId: actorProfileId,
                cancelReason: reason
            );
            var targetUserId = await GetOtherPartyUserIdAsync(appointmentId);
            await _notificationService.CreateAsync(
                targetUserId,
                NotificationType.Info,
                "An upcoming appointment has been cancelled."
            );
        }

        public async Task CompleteAppointmentAsync(Guid appointmentId)
        {
            var doctor = await _uow.Doctors.GetByUserIdAsync(_currentUser.UserId)
                ?? throw new UnauthorizedAccessException("Only doctors can complete appointments.");

            var appointment = await _uow.Appointments.GetByIdAsync(appointmentId)
                ?? throw new KeyNotFoundException("Appointment not found.");

            if (appointment.DoctorId != doctor.Id)
                throw new UnauthorizedAccessException("You can only complete your own appointments.");

            if (appointment.EndTime > DateTime.UtcNow)
                throw new InvalidOperationException("Cannot complete an appointment before its scheduled end time.");

            if (appointment.Status != AppointmentStatus.Accepted)
                throw new InvalidOperationException("Only accepted appointments can be marked as completed.");

            appointment.Status = AppointmentStatus.Completed;
            appointment.UpdatedBy = _currentUser.UserId;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _uow.SaveChangesAsync();

            var targetUserId = await GetOtherPartyUserIdAsync(appointmentId);

            await _notificationService.CreateAsync(
                targetUserId,
                NotificationType.Info,
                "Your appointment has been marked as completed."
            );
        }
        private async Task<Guid> GetOtherPartyUserIdAsync(Guid appointmentId)
        {
            var appointment = await _uow.Appointments.GetByIdAsync(appointmentId)
                ?? throw new KeyNotFoundException("Appointment not found.");

            var doctor = await _uow.Doctors.GetByIdAsync(appointment.DoctorId);
            var patient = await _uow.Patients.GetByIdAsync(appointment.PatientId);

            return _currentUser.UserId == doctor.UserId ? patient.UserId : doctor.UserId;
        }
    }
}