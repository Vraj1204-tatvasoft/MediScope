// File: MediScope.API/Controllers/PatientController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.Pagination;
namespace MediScope.API.Controllers
{
    [Route("api/patient")]
    [Authorize]
    public class PatientController : BaseController
    {
        private readonly IPatientService _patientService;
        private readonly IInvoiceService _invoiceService;

        public PatientController(IPatientService patientService, IInvoiceService invoiceService)
        {
            _patientService = patientService;
            _invoiceService = invoiceService;
        }

        //  GET /api/patient/profile
        /// <summary>Get logged-in patient's full profile</summary>
        [HttpGet("profile")]
        [Authorize(Policy = "PatientOnly")]
        public async Task<IActionResult> GetMyProfile()
        {
            var profile = await _patientService.GetMyProfileAsync(CurrentUserId);
            return Success(profile);
        }

        //  PUT /api/patient/profile 
        /// <summary>Update name, email, phone, blood group, gender, dob, address</summary>
        [HttpPut("profile")]
        [Authorize(Policy = "PatientOnly")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequestResponse("Invalid request data.");

            var profile = await _patientService.UpdateMyProfileAsync(CurrentUserId, request);
            return Success(profile, "Profile updated successfully.");
        }

        //  PATCH /api/patient/change-password 
        /// <summary>
        /// Change password — verifies current password.
        /// </summary>
        [HttpPatch("change-password")]
        [Authorize(Policy = "PatientOnly")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequestResponse("Invalid request data.");

            await _patientService.ChangePasswordAsync(CurrentUserId, request);
            return NoContent("Password changed successfully. Please log in again.");
        }

        [HttpGet("admin/all")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAllPatients([FromQuery] AdminPatientFilterDto filter, [FromQuery] PaginationParams pagination)
        {
            var response =
                await _patientService
                    .GetAdminPatientsAsync(filter, pagination);

            return Success(response);
        }
    }
}