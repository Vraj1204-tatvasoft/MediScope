using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;

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

            try
            {
                var newAppointmentId = await _appointmentService.CreateAppointmentAsync(request);
                return Success(new { AppointmentId = newAppointmentId }, "Appointment successfully scheduled.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequestResponse(ex.Message);
            }
        }

        [HttpPost("{id:guid}/respond")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> RespondToAppointment(Guid id, [FromBody] RespondToAppointmentRequestDto request)
        {
            if (id != request.AppointmentId) return BadRequestResponse("Route ID does not match request body ID.");
            if (!ModelState.IsValid) return BadRequestResponse("Invalid request.");

            try
            {
                await _appointmentService.RespondToAppointmentAsync(request);
                return Success($"Appointment successfully {request.Action.ToLower()}.");
            }
            catch (Exception ex)
            {
                return BadRequestResponse(ex.Message);
            }
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

        [HttpPost("{id:guid}/reschedule")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> RescheduleAppointment(Guid id, [FromBody] RespondToAppointmentRequestDto request)
        {
            if (id != request.AppointmentId)
                return BadRequestResponse("Route ID does not match request body ID.");

            if (!ModelState.IsValid)
                return BadRequestResponse("Invalid request.");

            try
            {
                await _appointmentService.RescheduleAppointmentAsync(request);

                return Success<object>(null, "Appointment successfully rescheduled to the new time slot.");
            }
            catch (ArgumentException ex)
            {
                return BadRequestResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequestResponse(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequestResponse(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequestResponse(ex.Message);
            }
        }
    }
}