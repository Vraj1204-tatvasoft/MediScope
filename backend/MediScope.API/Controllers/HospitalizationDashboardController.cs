using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.RequestDTO;

namespace MediScope.API.Controllers
{
    [ApiController]
    [Route("api/hospitalization-dashboard")]
    public class HospitalizationDashboardController : BaseController
    {
        private readonly IHospitalizationDashboardService _hospitalizationDashboardService;

        public HospitalizationDashboardController(IHospitalizationDashboardService hospitalizationDashboardService)
        {
            _hospitalizationDashboardService = hospitalizationDashboardService;
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetDashboard([FromQuery] HospitalizationDashboardFilterDto request)
        {
            var response = await _hospitalizationDashboardService.GetDashboardAsync(request);
            return Success(response);
        }
    }
}