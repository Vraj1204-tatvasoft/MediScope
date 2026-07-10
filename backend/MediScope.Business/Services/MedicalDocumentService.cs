// File: MediScope.Business/Services/MedicalDocumentService.cs

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using MediScope.Business.Hubs;
using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Entities;
using MediScope.Data.Repositories;
using MediScope.Common.Models.Enums;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediScope.Business.Services
{
    public class MedicalDocumentService : IMedicalDocumentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMedicalDocumentRepository _repository;
        private readonly IWebHostEnvironment _environment;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<RealtimeHub> _hubContext;
        private readonly IOcrService _ocrService;

        public MedicalDocumentService(
            IUnitOfWork uow,
            IMedicalDocumentRepository repository,
            IWebHostEnvironment environment,
            INotificationService notificationService,
            IHubContext<RealtimeHub> hubContext,
            IOcrService ocrService)
        {
            _uow = uow;
            _repository = repository;
            _environment = environment;
            _notificationService = notificationService;
            _hubContext = hubContext;
            _ocrService = ocrService;
        }

        public async Task UploadAsync(
            UploadDocumentCommand command,
            Guid patientUserId)
        {
            var allowedTypes = new[]
            {
                "application/pdf",
                "image/jpeg",
                "image/png",
                "image/jpg",
                "application/msword"
            };

            if (!allowedTypes.Contains(command.ContentType.ToLower()))
                throw new ArgumentException(
                    "Invalid file type. Allowed: PDF, JPG, PNG, DOC, DOCX.");

            const long maxBytes = 10 * 1024 * 1024;
            if (command.FileSizeBytes > maxBytes)
                throw new ArgumentException("File size must not exceed 10MB.");

            var patient = await _uow.Patients.GetByUserIdAsync(patientUserId)
                ?? throw new KeyNotFoundException("Patient not found.");

            var link = await _uow.DoctorPatients
                .GetExistingLinkAsync(command.DoctorId, patient.Id);

            if (link == null || link.Status != ConnectionStatus.Active)
                throw new InvalidOperationException(
                    "You can only share documents with your connected doctors.");

            var folder = Path.Combine(
                _environment.WebRootPath,
                "documents",
                patient.Id.ToString());

            Directory.CreateDirectory(folder);

            var extension = Path.GetExtension(command.FileName);
            var storedName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(folder, storedName);

            using var memoryStream = new MemoryStream();
            await command.FileStream.CopyToAsync(memoryStream);
            byte[] fileBytes = memoryStream.ToArray();

            await System.IO.File.WriteAllBytesAsync(fullPath, fileBytes);

            string extractedText = string.Empty;
            try
            {
                extractedText = _ocrService.ExtractTextFromFile(fileBytes, extension);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Warning] OCR Extraction failed: {ex.Message}");
            }

            var document = new MedicalDocument
            {
                PatientId = patient.Id,
                DoctorId = command.DoctorId,
                FileName = command.FileName,
                StoredName = storedName,
                FilePath = fullPath,
                ContentType = command.ContentType,
                FileSizeBytes = command.FileSizeBytes,
                Description = command.Description,
                Category = command.Category,
                ExtractedText = extractedText,
                CreatedBy = patientUserId,
                UpdatedBy = patientUserId,
            };

            await _repository.UploadDocumentAsync(document);

            var doctor = await _uow.Doctors.GetByIdWithDetailsAsync(command.DoctorId)
                ?? throw new KeyNotFoundException("Doctor not found.");

            // Notify the doctor — clicking navigates to /doctor/documents
            await _notificationService.CreateAsync(
                doctor.UserId,
                NotificationType.Info,
                $"{patient.User.FullName} uploaded a new document: {command.FileName}.",
                referenceType: "my-patients/patient.Id"
            );
        }

        public async Task<List<MedicalDocumentResponseDto>> GetPatientDocumentsAsync(
            Guid patientUserId)
        {
            var patient = await _uow.Patients.GetByUserIdAsync(patientUserId)
                ?? throw new KeyNotFoundException("Patient not found.");

            return await _repository.GetPatientDocumentsAsync(patient.Id);
        }

        public async Task<List<DoctorDocumentResponseDto>> GetDoctorDocumentsAsync(
            Guid doctorUserId)
        {
            var doctor = await _uow.Doctors.GetByUserIdAsync(doctorUserId)
                ?? throw new KeyNotFoundException("Doctor not found.");

            return await _repository.GetDoctorDocumentsAsync(doctor.Id);
        }

        public async Task AddFeedbackAsync(
            Guid doctorUserId,
            AddDocumentFeedbackRequestDto request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var doctor = await _uow.Doctors.GetByUserIdAsync(doctorUserId)
                ?? throw new KeyNotFoundException("Doctor not found.");

            await _repository.AddFeedbackAsync(
                request.DocumentId,
                request.Feedback ?? "No feedback provided.",
                (request.Severity ?? Severity.Normal).ToString());

            try
            {
                var document = await _repository.GetDocumentByIdAsync(request.DocumentId);
                if (document != null)
                {
                    var patient = await _uow.Patients.GetByIdAsync(document.PatientId);
                    if (patient != null)
                    {
                        string doctorName = doctor.User != null && !string.IsNullOrWhiteSpace(doctor.User.FullName)
                            ? $"Dr. {doctor.User.FullName}"
                            : "Your doctor";

                        // Notify the patient — clicking navigates to /patient/documents/:id
                        await _notificationService.CreateAsync(
                            patient.UserId,
                            NotificationType.Success,
                            $"{doctorName} reviewed your document and left feedback.",
                            referenceType: "my-doctors"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Warning] Failed to send document feedback notification: {ex.Message}");
            }
        }

        public async Task MarkViewedAsync(Guid doctorUserId, Guid documentId)
        {
            _ = await _uow.Doctors.GetByUserIdAsync(doctorUserId)
                ?? throw new KeyNotFoundException("Doctor not found.");

            await _repository.MarkViewedAsync(documentId);
        }

        public async Task<(FileStream Content, string ContentType, string FileName)> DownloadAsync(
            Guid userId,
            string role,
            Guid documentId)
        {
            var document = await _repository.GetDocumentByIdAsync(documentId)
                ?? throw new KeyNotFoundException("Document not found.");

            if (role == "Patient")
            {
                var patient = await _uow.Patients.GetByUserIdAsync(userId);
                if (patient == null || document.PatientId != patient.Id)
                    throw new UnauthorizedAccessException("You do not have permission to view this document.");
            }
            else if (role == "Doctor")
            {
                var doctor = await _uow.Doctors.GetByUserIdAsync(userId);
                if (doctor == null || document.DoctorId != doctor.Id)
                    throw new UnauthorizedAccessException("You do not have permission to view this document.");
            }

            if (!System.IO.File.Exists(document.FilePath))
                throw new FileNotFoundException("The physical file could not be found on the server.");

            var stream = new FileStream(document.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            var contentType = string.IsNullOrEmpty(document.ContentType)
                ? "application/octet-stream"
                : document.ContentType;

            return (stream, contentType, document.FileName);
        }
    }
}