using Microsoft.EntityFrameworkCore;
using Npgsql;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.DTOs.Request;

namespace MediScope.Data.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly AppDbContext _context;

        public AppointmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateAppointmentViaSqlAsync(Guid doctorId, Guid patientId, DateTime startTime, int durationMinutes, string? doctorNotes, Guid createdBy)
        {
            try
            {
                var appointmentId = await _context.Database
                    .SqlQuery<Guid>($"SELECT fn_create_appointment({doctorId}, {patientId}, {startTime}, {durationMinutes}, {doctorNotes}, {createdBy}) AS \"Value\"")
                    .SingleAsync();

                return appointmentId;
            }
            catch (PostgresException ex) when (ex.MessageText.Contains("OVERLAP_ERROR"))
            {
                throw new InvalidOperationException("The requested time slot overlaps with an existing appointment.");
            }
        }

        public async Task<bool> RespondToAppointmentViaSqlAsync(Guid appointmentId, Guid patientId, string action, string? patientNotes, Guid updatedBy)
        {
            try
            {
                var success = await _context.Database
                    .SqlQuery<bool>($"SELECT fn_respond_appointment({appointmentId}, {patientId}, {action.ToLower()}, {patientNotes}, {updatedBy}) AS \"Value\"")
                    .SingleAsync();

                return success;
            }
            catch (PostgresException ex) when (ex.MessageText.Contains("NOT_FOUND") || ex.MessageText.Contains("INVALID_STATE"))
            {
                throw new InvalidOperationException(ex.MessageText);
            }
        }
        public async Task<List<DoctorSlotResponseDto>> GetDoctorScheduleAsync(Guid doctorId)
        {
            return await _context.Database
                .SqlQuery<DoctorSlotResponseDto>($"SELECT * FROM fn_get_doctor_schedule({doctorId})")
                .ToListAsync();
        }
        public async Task<List<PatientAppointmentResponseDto>> GetPatientAppointmentsAsync(Guid patientId)
        {
            return await _context.Database
                .SqlQuery<PatientAppointmentResponseDto>($"SELECT * FROM fn_get_patient_appointments({patientId})")
                .ToListAsync();
        }
        public async Task<bool> RescheduleAppointmentViaSqlAsync(Guid appointmentId, Guid patientId, DateTime newStartTime, string? rescheduleReason, Guid updatedBy)
        {
            try
            {
                var success = await _context.Database
                    .SqlQuery<bool>($"SELECT fn_reschedule_appointment({appointmentId}, {patientId}, {newStartTime}, {rescheduleReason}, {updatedBy}) AS \"Value\"")
                    .SingleAsync();

                return success;
            }
            catch (PostgresException ex) when (ex.MessageText.Contains("OVERLAP_ERROR"))
            {
                throw new InvalidOperationException("The requested reschedule time overlaps with another appointment. Please choose a different time.");
            }
            catch (PostgresException ex) when (ex.MessageText.Contains("NOT_FOUND"))
            {
                throw new KeyNotFoundException("Appointment not found or you do not have permission to modify it.");
            }
        }
    }
}