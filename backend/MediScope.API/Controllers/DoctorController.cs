using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MediScope.Business.Services.Interfaces;

using MediScope.Common.Models.DTOs.Request;

namespace MediScope.API.Controllers
{
    [ApiController]
    [Route("api/doctor")]
    [Authorize]
    public class DoctorController : BaseController
    {
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        // ── CREATE DOCTOR — Admin only ───────────────────────────────
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreateDoctor(
            [FromBody] CreateDoctorRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequestResponse("Invalid request.");

            var doctor =
                await _doctorService.CreateDoctorAsync(request);

            return Success(
                doctor,
                "Doctor created successfully.");
        }

        // ── GET ALL DOCTORS — Admin + Patient ───────────────────────
        [HttpGet("all")]
        [Authorize(Policy = "PatientOrAdmin")]
        public async Task<IActionResult> GetAllDoctors()
        {
            var doctors =
                await _doctorService.GetAllDoctorsAsync();

            return Success(doctors);
        }

        // ── GET DOCTOR BY ID — Admin + Patient ──────────────────────
        [HttpGet("{doctorId:guid}")]
        [Authorize(Policy = "PatientOrAdmin")]
        public async Task<IActionResult> GetDoctorById(Guid doctorId)
        {
            var doctor =
                await _doctorService.GetDoctorByIdAsync(doctorId);

            return Success(doctor);
        }

        // ── GET MY PROFILE — Doctor only ────────────────────────────
        [HttpGet("me")]
        [Authorize(Policy = "DoctorOrAdmin")]
        public async Task<IActionResult> GetMyProfile()
        {
            var doctor =
                await _doctorService.GetMyProfileAsync(CurrentUserId);

            return Success(doctor);
        }

        // ── UPDATE MY PROFILE — Doctor only ─────────────────────────
        [HttpPut]
        [Authorize(Policy = "DoctorOnly")]
        public async Task<IActionResult> UpdateMyProfile(
            [FromBody] UpdateDoctorRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequestResponse("Invalid request.");

            var updatedDoctor =
                await _doctorService.UpdateMyProfileAsync(
                    CurrentUserId,
                    request);

            return Success(
                updatedDoctor,
                "Profile updated successfully.");
        }

        [HttpPatch("change-password")]
        [Authorize(Policy = "DoctorOnly")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequestResponse("Invalid request data.");

            await _doctorService.ChangePasswordAsync(CurrentUserId, request);
            return NoContent("Password changed successfully. Please log in again.");
        }
    }
}