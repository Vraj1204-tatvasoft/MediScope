// File: MediScope.API/Controllers/DoctorPatientController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;

namespace MediScope.API.Controllers
{
    [ApiController]
    [Route("api/doctor-patient")]
    [Authorize]
    public class DoctorPatientController : BaseController
    {
        private readonly IDoctorPatientService _doctorPatientService;

        public DoctorPatientController(IDoctorPatientService doctorPatientService)
        {
            _doctorPatientService = doctorPatientService;
        }

        // PATIENT ENDPOINTS

        // POST api/doctor-patient/request
        // Patient sends request (DoctorId optional)
        [HttpPost("request")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> SendRequest([FromBody] SendDoctorRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequestResponse("Invalid request data.");

            var response = await _doctorPatientService
                .SendRequestAsync(CurrentUserId, request);

            return Success(response, "Request submitted successfully. Awaiting admin review.");
        }

        // GET api/doctor-patient/my-doctors
        [HttpGet("my-doctors")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetMyDoctors()
        {
            var response = await _doctorPatientService.GetMyDoctorsAsync(CurrentUserId);
            return Success(response);
        }

        // PATCH api/doctor-patient/revoke
        [HttpPatch("revoke")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> RevokeAccess([FromBody] RevokeAccessDto request)
        {
            if (!ModelState.IsValid)
                return BadRequestResponse("Invalid request data.");

            await _doctorPatientService.RevokeAccessAsync(CurrentUserId, request);
            return NoContent("Doctor access revoked successfully.");
        }

        // ADMIN ENDPOINTS

        // GET api/doctor-patient/admin/pending
        // Admin views requests waiting for their review
        [HttpGet("admin/pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingAdminRequests()
        {
            var response = await _doctorPatientService.GetPendingAdminRequestsAsync();
            return Success(response);
        }

        // GET api/doctor-patient/admin/all
        // Admin views all requests with optional filters
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllRequestsForAdmin(
            [FromQuery] AdminDoctorPatientFilterDto filter)
        {
            var response = await _doctorPatientService
                .GetAllRequestsForAdminAsync(filter);
            return Success(response);
        }

        // PATCH api/doctor-patient/admin/approve
        // Admin approves and assigns a doctor
        [HttpPatch("admin/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveRequest([FromBody] AdminApproveRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequestResponse("Invalid request data.");

            var response = await _doctorPatientService
                .ApproveRequestAsync(CurrentUserId, request);

            return Success(response, "Request approved and doctor assigned.");
        }

        // PATCH api/doctor-patient/admin/reject
        // Admin rejects a request
        [HttpPatch("admin/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectRequest([FromBody] AdminRejectRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequestResponse("Invalid request data.");

            await _doctorPatientService
                .RejectRequestAsync(CurrentUserId, request);

            return NoContent("Request rejected.");
        }

        // DOCTOR ENDPOINTS

        // GET api/doctor-patient/pending
        // Doctor views requests waiting for their acceptance
        [HttpGet("pending")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var response = await _doctorPatientService
                .GetPendingRequestsAsync(CurrentUserId);
            return Success(response);
        }

        // PATCH api/doctor-patient/respond
        // Doctor accepts or declines
        [HttpPatch("respond")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> RespondToRequest([FromBody] RespondToRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequestResponse("Invalid request data.");

            var response = await _doctorPatientService
                .RespondToRequestAsync(CurrentUserId, request);

            return Success(
                response,
                request.Accept
                    ? "Patient accepted successfully."
                    : "Patient declined successfully.");
        }

        // GET api/doctor-patient/my-patients
        [HttpGet("my-patients")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetMyPatients()
        {
            var response = await _doctorPatientService
                .GetMyPatientsAsync(CurrentUserId);
            return Success(response);
        }
    }
}