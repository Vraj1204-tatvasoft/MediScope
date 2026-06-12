using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MediScope.Business.Services.Interfaces;

using MediScope.Common.Models.DTOs.Request;

namespace MediScope.API.Controllers
{
    [ApiController]
    [Route("api/metric-definitions")]
    [Authorize]
    public class MetricDefinitionController : BaseController
    {
        private readonly IMetricDefinitionService
            _metricDefinitionService;

        public MetricDefinitionController(
            IMetricDefinitionService metricDefinitionService)
        {
            _metricDefinitionService =
                metricDefinitionService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateMetricDefinitionRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequestResponse(
                    "Invalid request.");

            var result =
                await _metricDefinitionService
                    .CreateAsync(request);

            return Success(
                result,
                "Metric definition created successfully.");
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetAll()
        {
            var result =
                await _metricDefinitionService
                    .GetAllAsync();

            return Success(result);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result =
                await _metricDefinitionService
                    .GetByIdAsync(id);

            return Success(result);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMetricDefinitionRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequestResponse(
                    "Invalid request.");

            var result =
                await _metricDefinitionService
                    .UpdateAsync(id, request);

            return Success(
                result,
                "Metric definition updated successfully.");
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _metricDefinitionService
                .DeleteAsync(id);

            return NoContent(
                "Metric definition deleted successfully.");
        }
    }
}