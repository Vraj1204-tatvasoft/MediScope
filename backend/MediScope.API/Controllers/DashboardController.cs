using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MediScope.Business.Services.Interfaces;

namespace MediScope.API.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : BaseController
    {
        private readonly IPatientDashboardService _patientDashboardService;
        private readonly IAdminDashboardService _adminDashboardService;
        private readonly IDoctorDashboardService _doctorDashboardService;

        public DashboardController(IPatientDashboardService patientDashboardService, IAdminDashboardService adminDashboardService, IDoctorDashboardService doctorDashboardService)
        {
            _patientDashboardService = patientDashboardService;
            _adminDashboardService = adminDashboardService;
            _doctorDashboardService = doctorDashboardService;
        }

        [HttpGet("patient")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetPatientDashboard()
        {
            var result =
                await _patientDashboardService
                    .GetDashboardAsync(CurrentUserId);

            return Success(result);
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            var result = await _adminDashboardService
                .GetDashboardAsync(CurrentUserId);

            return Success(result);
        }

        [HttpGet("doctor")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetDoctorDashboard()
        {
            var result = await _doctorDashboardService
                .GetDashboardAsync(CurrentUserId);

            return Success(result);
        }

        [HttpGet("doctor/vital-trends")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetVitalTrends(
            [FromQuery] string metricType = "heart_rate",
            [FromQuery] string patientId = "all",
            [FromQuery] string duration = "last_month",
            [FromQuery] string? fromDate = null,
            [FromQuery] string? toDate = null)
        {
            DateTime? parsedFrom = string.IsNullOrWhiteSpace(fromDate)
            ? null
            : DateTime.Parse(fromDate).ToUniversalTime();

            DateTime? parsedTo = string.IsNullOrWhiteSpace(toDate)
            ? null
            : DateTime.Parse(toDate).Date.AddHours(23).AddMinutes(59).AddSeconds(59).ToUniversalTime();

            var result = await _doctorDashboardService.GetVitalTrendsAsync(
                CurrentUserId, metricType, patientId, duration, parsedFrom, parsedTo);
            return Success(result);
        }
    }
}