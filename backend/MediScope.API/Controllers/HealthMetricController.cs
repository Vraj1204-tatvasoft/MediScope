using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.Pagination;

namespace MediScope.API.Controllers
{
    [Route("api/health-metrics")]
    [Authorize]
    public class HealthMetricController : BaseController
    {
        private readonly IHealthMetricService
            _healthMetricService;

        public HealthMetricController(
            IHealthMetricService healthMetricService)
        {
            _healthMetricService =
                healthMetricService;
        }

        // ─────────────────────────────────────────────
        // ADD HEALTH RECORD
        // ─────────────────────────────────────────────

        [HttpPost]
        [Authorize(Roles = "Patient,Doctor")]
        public async Task<IActionResult> AddMetric([FromBody] AddHealthMetricRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequestResponse(
                    "Invalid request data.");
            }

            var result =
                await _healthMetricService
                    .AddMetricAsync(
                        request,
                        CurrentUserId,
                        CurrentUserRole);

            return Created(
                result,
                "Health metric recorded successfully.");
        }

        // ─────────────────────────────────────────────
        // GET SINGLE SUBMISSION
        // ─────────────────────────────────────────────

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Patient,Doctor,Admin")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result =
                await _healthMetricService
                    .GetByIdAsync(
                        id,
                        CurrentUserId,
                        CurrentUserRole);

            return Success(result);
        }

        // GET ALL HISTORY BY PATIENT

        [HttpGet("patient/{patientId:guid}")]
        [Authorize(Roles = "Patient,Doctor,Admin")]
        public async Task<IActionResult> GetAllByPatient(
            Guid patientId, [FromQuery] PaginationParams pagination)
        {
            var result =
                await _healthMetricService
                    .GetAllByPatientAsync(
                        patientId,
                        pagination,
                        CurrentUserId,
                        CurrentUserRole);

            return Success(result);
        }

        // GET LOGGED-IN PATIENT HISTORY

        [HttpGet("me/paged")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult>
            GetMyPagedMetrics([FromQuery] PaginationParams pagination)
        {
            var result =
                await _healthMetricService
                    .GetPagedForLoggedInPatientAsync(
                        CurrentUserId,
                        pagination);

            return Success(result);
        }

        // Delete that particular submission
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Patient,Doctor")]
        public async Task<IActionResult> DeleteSubmission(Guid id)
        {
            await _healthMetricService.DeleteSubmissionAsync(id, CurrentUserId, CurrentUserRole);

            return Success(true, "Health record submission data cleared successfully.");
        }
    }
}