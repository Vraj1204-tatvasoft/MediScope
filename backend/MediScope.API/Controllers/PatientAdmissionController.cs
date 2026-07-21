using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.Pagination;

namespace MediScope.API.Controllers
{
    [ApiController]
    [Route("api/admissions")]
    public class PatientAdmissionController : BaseController
    {
        private readonly IPatientAdmissionService _admissionService;

        public PatientAdmissionController(IPatientAdmissionService admissionService)
        {
            _admissionService = admissionService;
        }

        [HttpGet]
        [Authorize(Policy = "PatientOrAdmin")]
        public async Task<IActionResult> GetAdmissions([FromQuery] PaginationParams request)
        {
            var admissions = await _admissionService.GetAdmissionsPagedAsync(request);
            return Success(admissions);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> AdmitPatient([FromBody] AdmitPatientRequestDto request)
        {
            await _admissionService.AdmitPatientAsync(request);
            return NoContent("Patient admitted successfully.");
        }

        [HttpPost("{id:guid}/transfer")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> TransferPatientBed(Guid id, [FromBody] TransferBedRequestDto request)
        {
            await _admissionService.TransferPatientBedAsync(id, request);
            return NoContent("Patient transferred successfully.");
        }

        [HttpPost("{id:guid}/discharge")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DischargePatient(Guid id, [FromBody] DischargePatientRequestDto request)
        {
            await _admissionService.DischargePatientAsync(id, request);
            return NoContent("Patient discharged successfully.");
        }
        [HttpGet("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAdmission(Guid id)
        {
            var response = await _admissionService.GetAdmissionByIdAsync(id);
            return Success(response);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateAdmission(Guid id, [FromBody] UpdateAdmissionRequestDto request)
        {
            await _admissionService.UpdateAdmissionAsync(id, request);
            return NoContent("Admission updated successfully.");
        }

        [HttpGet("{roomId}/active-patients")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetActivePatientsByRoom(Guid roomId)
        {
            var response =
                await _admissionService.GetActivePatientsByRoomAsync(roomId);

            return Success(response);
        }
    }
}