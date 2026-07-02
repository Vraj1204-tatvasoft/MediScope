using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using System;
using System.Threading.Tasks;

namespace MediScope.API.Controllers
{
    [ApiController]
    [Route("api/appointments")]
    [Authorize]
    public class AppointmentController : BaseController
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequestResponse("Invalid request.");

            var newAppointmentId = await _appointmentService.CreateAppointmentAsync(request);
            return Success(new { AppointmentId = newAppointmentId }, "Appointment successfully scheduled.");
        }

        [HttpPost("{id:guid}/respond")]
        [Authorize(Roles = "Doctor, Patient")]
        public async Task<IActionResult> RespondToAppointment(Guid id, [FromBody] RespondToAppointmentRequestDto request)
        {
            if (id != request.AppointmentId) return BadRequestResponse("Route ID does not match request body ID.");
            if (!ModelState.IsValid) return BadRequestResponse("Invalid request.");

            await _appointmentService.RespondToAppointmentAsync(request);
            return Success<object>(null, $"Appointment successfully {request.Action.ToLower()}.");
        }

        [HttpGet("doctor/my-schedule")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetDoctorSchedule()
        {
            var result = await _appointmentService.GetMyDoctorScheduleAsync();
            return Success(result);
        }

        [HttpGet("patient/my-appointments")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetPatientAppointments()
        {
            var result = await _appointmentService.GetMyPatientAppointmentsAsync();
            return Success(result);
        }

        [HttpGet("patient/{patientId:guid}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetPatientAppointmentsForDoctor(Guid patientId)
        {
            var result = await _appointmentService.GetAppointmentsByPatientForDoctorAsync(patientId);
            return Success(result);
        }

        [HttpPost("{id:guid}/reschedule")]
        [Authorize(Roles = "Doctor, Patient")]
        public async Task<IActionResult> RescheduleAppointment(Guid id, [FromBody] RescheduleAppointmentRequestDto request)
        {
            if (id != request.AppointmentId) return BadRequestResponse("Route ID does not match request body ID.");
            if (!ModelState.IsValid) return BadRequestResponse("Invalid request.");

            await _appointmentService.RescheduleAppointmentAsync(request);
            return Success<object>(null, "Reschedule request successfully submitted.");
        }

        [HttpPost("{id:guid}/cancel")]
        [Authorize(Roles = "Doctor, Patient")]
        public async Task<IActionResult> CancelAppointment(Guid id, [FromBody] CancelAppointmentRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequestResponse("Invalid request.");

            await _appointmentService.CancelAppointmentAsync(id, request.Reason);
            return Success<object>(null, "Appointment successfully cancelled.");
        }

        [HttpPost("{id:guid}/complete")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> CompleteAppointment(Guid id)
        {
            await _appointmentService.CompleteAppointmentAsync(id);
            return Success<object>(null, "Appointment marked as completed successfully.");
        }
    }
}