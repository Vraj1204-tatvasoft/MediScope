using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MediScope.Business.Services.Interfaces;

using MediScope.Common.Models.DTOs.Request;

namespace MediScope.API.Controllers
{
    [Route("api/documents")]
    [Authorize]
    public class MedicalDocumentController : BaseController
    {
        private readonly IMedicalDocumentService _documentService;

        public MedicalDocumentController(IMedicalDocumentService documentService)
        {
            _documentService = documentService;
        }

        // PATIENT → UPLOAD DOCUMENT

        [HttpPost]
        [Authorize(Roles = "Patient")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] UploadDocumentRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequestResponse("Invalid request data.");

            if (request.File == null || request.File.Length == 0)
                return BadRequestResponse("No file was provided.");

            // Business layer never touches IFormFile directly
            var command = new UploadDocumentCommand
            {
                DoctorId = request.DoctorId,
                Description = request.Description ?? string.Empty,
                Category = request.Category,
                FileStream = request.File.OpenReadStream(),
                FileName = request.File.FileName,
                ContentType = request.File.ContentType,
                FileSizeBytes = request.File.Length,
            };

            await _documentService.UploadAsync(command, CurrentUserId);

            return Created(true, "Document uploaded successfully.");
        }

        // PATIENT → MY DOCUMENTS

        [HttpGet("my")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetMyDocuments()
        {
            var result =
                await _documentService
                    .GetPatientDocumentsAsync(
                        CurrentUserId);

            return Success(result);
        }

        // DOCTOR → DOCUMENTS ASSIGNED TO ME

        [HttpGet("doctor")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetDoctorDocuments()
        {
            var result =
                await _documentService
                    .GetDoctorDocumentsAsync(
                        CurrentUserId);

            return Success(result);
        }

        // DOCTOR → MARK VIEWED

        [HttpPost("{documentId:guid}/view")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> MarkViewed(
            Guid documentId)
        {
            await _documentService
                .MarkViewedAsync(
                    CurrentUserId,
                    documentId);

            return Success(
                true,
                "Document marked as viewed.");
        }

        // DOCTOR → ADD FEEDBACK

        [HttpPost("feedback")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> AddFeedback(
            [FromBody]
            AddDocumentFeedbackRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequestResponse(
                    "Invalid request.");

            await _documentService
                .AddFeedbackAsync(
                    CurrentUserId,
                    request);

            return Success(
                true,
                "Feedback submitted successfully.");
        }

        // DOWNLOAD DOCUMENT

        [HttpGet("{documentId:guid}/download")]
        [Authorize(Roles = "Patient,Doctor")]
        public async Task<IActionResult> Download(Guid documentId)
        {
            var file = await _documentService.DownloadAsync(CurrentUserId, CurrentUserRole, documentId);

            return File(file.Content, file.ContentType, file.FileName);
        }

        // PATIENT → DELETE DOCUMENT

        /*[HttpDelete("{documentId:guid}")]
        [Authorize(Roles = "Patient")]
        public Task<IActionResult> Delete(
            Guid documentId)
            => HandleAsync(async () =>
            {
                await _documentService
                    .DeleteAsync(
                        CurrentUserId,
                        documentId);

                return Success(
                    true,
                    "Document deleted successfully.");
            });*/
    }
}