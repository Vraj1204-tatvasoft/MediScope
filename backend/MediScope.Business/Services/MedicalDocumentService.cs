// File: MediScope.Business/Services/MedicalDocumentService.cs

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using MediScope.Business.Hubs;
using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Entities;
using MediScope.Data.Repositories;

namespace MediScope.Business.Services
{
    public class MedicalDocumentService : IMedicalDocumentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMedicalDocumentRepository _repository;
        private readonly IWebHostEnvironment _environment;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<RealtimeHub> _hubContext;      //  FIX: inject SignalR

        public MedicalDocumentService(
            IUnitOfWork uow,
            IMedicalDocumentRepository repository,
            IWebHostEnvironment environment,
            INotificationService notificationService,
            IHubContext<RealtimeHub> hubContext)           //  FIX: add to constructor
        {
            _uow = uow;
            _repository = repository;
            _environment = environment;
            _notificationService = notificationService;
            _hubContext = hubContext;               //  FIX: assign
        }

        // ── UPLOAD ────────────────────────────────────────────────────
        // accepts UploadDocumentCommand (no IFormFile) instead of UploadDocumentRequestDto
        public async Task UploadAsync(
            UploadDocumentCommand command,
            Guid patientUserId)
        {
            // ── Validate file type ────────────────────────────────────
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

            // ── Validate file size (max 10MB) ─────────────────────────
            const long maxBytes = 10 * 1024 * 1024;
            if (command.FileSizeBytes > maxBytes)
                throw new ArgumentException("File size must not exceed 10MB.");

            // ── Validate patient ──────────────────────────────────────
            var patient = await _uow.Patients.GetByUserIdAsync(patientUserId)
                ?? throw new KeyNotFoundException("Patient not found.");

            // ── Validate doctor is actively connected ─────────────────
            var link = await _uow.DoctorPatients
                .GetExistingLinkAsync(command.DoctorId, patient.Id);

            if (link == null || link.Status != "active")
                throw new InvalidOperationException(
                    "You can only share documents with your connected doctors.");

            // ── Save file to disk ─────────────────────────────────────
            var folder = Path.Combine(
                _environment.WebRootPath,
                "documents",
                patient.Id.ToString());

            Directory.CreateDirectory(folder);

            var extension = Path.GetExtension(command.FileName);
            var storedName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(folder, storedName);

            //  FIX: write from Stream — no IFormFile needed
            await using var fileStream = new FileStream(fullPath, FileMode.Create);
            await command.FileStream.CopyToAsync(fileStream);

            // ── Save metadata via stored procedure ────────────────────
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
                CreatedBy = patientUserId,
                UpdatedBy = patientUserId,
            };

            await _repository.UploadDocumentAsync(document);

            // ── Notify doctor ─────────────────────────────────────────
            var doctor = await _uow.Doctors.GetByIdWithDetailsAsync(command.DoctorId)
                ?? throw new KeyNotFoundException("Doctor not found.");

            // DB notification
            await _notificationService.CreateAsync(
                doctor.UserId,
                "info",
                $"{patient.User.FullName} uploaded a new document: {command.FileName}.");

        }

        // ── GET PATIENT DOCUMENTS ─────────────────────────────────────
        public async Task<List<MedicalDocumentResponseDto>> GetPatientDocumentsAsync(
            Guid patientUserId)
        {
            var patient = await _uow.Patients.GetByUserIdAsync(patientUserId)
                ?? throw new KeyNotFoundException("Patient not found.");

            return await _repository.GetPatientDocumentsAsync(patient.Id);
        }

        // ── GET DOCTOR DOCUMENTS ──────────────────────────────────────
        public async Task<List<DoctorDocumentResponseDto>> GetDoctorDocumentsAsync(
            Guid doctorUserId)
        {
            var doctor = await _uow.Doctors.GetByUserIdAsync(doctorUserId)
                ?? throw new KeyNotFoundException("Doctor not found.");

            return await _repository.GetDoctorDocumentsAsync(doctor.Id);
        }

        // ── ADD FEEDBACK ──────────────────────────────────────────────
        public async Task AddFeedbackAsync(
            Guid doctorUserId,
            AddDocumentFeedbackRequestDto request)
        {
            // 1. Ensure the request itself is not null
            if (request == null) throw new ArgumentNullException(nameof(request));

            // 2. Get the doctor
            var doctor = await _uow.Doctors.GetByUserIdAsync(doctorUserId)
                ?? throw new KeyNotFoundException("Doctor not found.");

            // 3. Save the feedback to the database
            await _repository.AddFeedbackAsync(
                request.DocumentId,
                request.Feedback ?? "No feedback provided.",
                request.Severity ?? "Normal");

            // 4. Safely trigger the notification without crashing the app
            try
            {
                var document = await _repository.GetDocumentByIdAsync(request.DocumentId);
                if (document != null)
                {
                    // Fetch the patient directly from the UOW to guarantee it isn't null
                    var patient = await _uow.Patients.GetByIdAsync(document.PatientId);
                    if (patient != null)
                    {
                        // Safely format the doctor's name, falling back to "Your doctor" if User property is missing
                        string doctorName = doctor.User != null && !string.IsNullOrWhiteSpace(doctor.User.FullName)
                            ? $"Dr. {doctor.User.FullName}"
                            : "Your doctor";

                        await _notificationService.CreateAsync(
                            patient.UserId, // Use the Patient's underlying Account ID
                            "success",
                            $"{doctorName} reviewed your document and left feedback.");
                    }
                }
            }
            catch (Exception ex)
            {
                // If the notification fails (e.g. SignalR issue), log it, but DO NOT crash the feedback submission
                Console.WriteLine($"[Warning] Failed to send document feedback notification: {ex.Message}");
            }
        }

        // ── MARK VIEWED ───────────────────────────────────────────────
        public async Task MarkViewedAsync(Guid doctorUserId, Guid documentId)
        {
            // FIX: KeyNotFoundException instead of generic Exception
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

            // Security Check: Ensure the user actually has rights to this document
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

            // Verify the physical file actually exists on the server disk
            if (!System.IO.File.Exists(document.FilePath))
                throw new FileNotFoundException("The physical file could not be found on the server.");

            // Open the file stream to send back to the frontend
            var stream = new FileStream(document.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            // Fallback if ContentType was null
            var contentType = string.IsNullOrEmpty(document.ContentType)
                ? "application/octet-stream"
                : document.ContentType;

            return (stream, contentType, document.FileName);
        }
    }
}