using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Entities;

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
                return await _context.Database
                    .SqlQuery<Guid>($"SELECT fn_create_appointment({doctorId}, {patientId}, {startTime}, {durationMinutes}, {doctorNotes}, {createdBy}) AS \"Value\"")
                    .SingleAsync();
            }
            catch (PostgresException ex) when (ex.MessageText.Contains("OVERLAP_ERROR"))
            {
                throw new InvalidOperationException("The requested time slot overlaps with an existing appointment.");
            }
        }

        public async Task<bool> RespondToAppointmentViaSqlAsync(Guid appointmentId, string action, string? notes, Guid userId)
        {
            try
            {
                return await _context.Database
                    .SqlQuery<bool>($"SELECT fn_respond_appointment({appointmentId}, {action.ToLower()}, {notes}, {userId}) AS \"Value\"")
                    .SingleAsync();
            }
            catch (PostgresException ex) when (ex.MessageText.Contains("OVERLAP_ERROR"))
            {
                throw new InvalidOperationException("This time slot was booked by someone else while the request was pending.");
            }
            catch (PostgresException ex) when (ex.MessageText.Contains("NOT_FOUND") || ex.MessageText.Contains("INVALID_STATE") || ex.MessageText.Contains("INVALID_ACTION"))
            {
                throw new InvalidOperationException(ex.MessageText);
            }
        }

        public async Task<List<DoctorAppointmentResponseDto>> GetDoctorScheduleAsync(Guid doctorId)
        {
            return await _context.Database
                .SqlQuery<DoctorAppointmentResponseDto>($"SELECT * FROM fn_get_doctor_schedule({doctorId})")
                .ToListAsync();
        }

        public async Task<List<PatientAppointmentResponseDto>> GetPatientAppointmentsAsync(Guid patientId)
        {
            return await _context.Database
                .SqlQuery<PatientAppointmentResponseDto>($"SELECT * FROM fn_get_patient_appointments({patientId})")
                .ToListAsync();
        }
        public async Task<bool> RequestRescheduleViaSqlAsync(Guid appointmentId, Guid userId, DateTime newStartTime, string? rescheduleReason)
        {
            try
            {
                return await _context.Database
                    .SqlQuery<bool>($"SELECT fn_request_reschedule({appointmentId}, {userId}, {newStartTime}, {rescheduleReason}) AS \"Value\"")
                    .SingleAsync();
            }
            catch (PostgresException ex) when (ex.MessageText != null && ex.MessageText.Contains("CONFLICT"))
            {
                string errorMessage = ex.MessageText.Replace("CONFLICT: ", "");
                throw new InvalidOperationException(errorMessage);
            }
        }

        public async Task<bool> CancelAppointmentViaSqlAsync(Guid appointmentId, Guid actorId, string? cancelReason)
        {
            try
            {
                return await _context.Database
                    .SqlQuery<bool>($"SELECT fn_cancel_appointment({appointmentId}, {actorId}, {cancelReason}) AS \"Value\"")
                    .SingleAsync();
            }
            catch (PostgresException ex) when (ex.MessageText.Contains("INVALID_STATE") || ex.MessageText.Contains("NOT_FOUND"))
            {
                throw new InvalidOperationException(ex.MessageText);
            }
        }
        public async Task<Appointment?> GetByIdAsync(Guid id)
        {
            return await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        }
        public async Task<List<Appointment>> GetAppointmentsByPatientAndDoctorAsync(Guid patientId, Guid doctorId)
        {
            return await _context.Appointments
                .Where(a => a.PatientId == patientId && a.DoctorId == doctorId && !a.IsDeleted)
                .OrderByDescending(a => a.StartTime)
                .ToListAsync();
        }
    }
}