using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MediScope.Business.Hubs;
using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Entities;
using MediScope.Data.Repositories;
using MediScope.Common.Models.Enums;

namespace MediScope.Business.Services
{
    public class DoctorPatientService : IDoctorPatientService
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;
        private readonly IHubContext<RealtimeHub> _hubContext;
        private readonly INotificationService _notificationService;

        public DoctorPatientService(
            IUnitOfWork uow,
            ICurrentUserService currentUser,
            IHubContext<RealtimeHub> hubContext,
            INotificationService notificationService)
        {
            _uow = uow;
            _currentUser = currentUser;
            _hubContext = hubContext;
            _notificationService = notificationService;
        }

        // ════════════════════════════════════════════════════════════
        // PATIENT: Send request (with or without selecting a doctor)
        // ════════════════════════════════════════════════════════════
        public async Task<PatientDoctorResponseDto> SendRequestAsync(
            Guid patientUserId, SendDoctorRequestDto request)
        {
            var patient = await _uow.Patients
                .GetFirstOrDefaultAsync(
                    p => p.UserId == patientUserId && !p.IsDeleted,
                    p => p.User)
                ?? throw new KeyNotFoundException("Patient profile not found.");

            // If doctor selected — validate they exist and are active
            if (request.DoctorId.HasValue)
            {
                var doctor = await _uow.Doctors
                    .GetByIdWithDetailsAsync(request.DoctorId.Value)
                    ?? throw new KeyNotFoundException("Doctor not found.");

                if (!doctor.User.IsActive)
                    throw new InvalidOperationException(
                        "This doctor is not currently active.");
            }

            // Check for existing non-terminal link
            var existing = await _uow.DoctorPatients
                .GetExistingLinkAsync(request.DoctorId, patient.Id);

            DoctorPatient link;

            if (existing != null)
            {
                if (existing.Status == ConnectionStatus.PendingAdmin)
                    throw new InvalidOperationException(
                        "You already have a pending request awaiting admin review.");

                if (existing.Status == ConnectionStatus.PendingDoctor)
                    throw new InvalidOperationException(
                        "Your request has already been approved and is awaiting doctor acceptance.");

                if (existing.Status == ConnectionStatus.Active)
                    throw new InvalidOperationException(
                        "You are already connected with this doctor.");

                // Re-request after declined/rejected/revoked — reuse row
                existing.Status = ConnectionStatus.PendingAdmin;
                existing.DoctorId = request.DoctorId;
                existing.RequestedAt = DateTime.UtcNow;
                existing.AssignedAt = null;
                existing.RevokedAt = null;
                existing.AdminReviewedAt = null;
                existing.ReviewedByAdminId = null;
                existing.AdminNote = null;
                existing.IsDeleted = false;
                existing.DeletedAt = null;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = patientUserId;

                _uow.DoctorPatients.Update(existing);
                await _uow.SaveChangesAsync();

                link = await _uow.DoctorPatients
                    .GetByIdWithDetailsAsync(existing.Id)
                    ?? throw new Exception("Failed to reload link.");
            }
            else
            {
                // Create new link
                link = new DoctorPatient
                {
                    PatientId = patient.Id,
                    DoctorId = request.DoctorId,   // may be null
                    Status = ConnectionStatus.PendingAdmin,
                    RequestedAt = DateTime.UtcNow,
                    CreatedBy = patientUserId,
                    UpdatedBy = patientUserId,
                };

                await _uow.DoctorPatients.AddAsync(link);
                await _uow.SaveChangesAsync();

                link = await _uow.DoctorPatients
                    .GetByIdWithDetailsAsync(link.Id)
                    ?? throw new Exception("Failed to reload link.");
            }

            // Notify all admins about new request
            await NotifyAdminsAsync(
                $"{patient.User.FullName} submitted a new doctor connection request.");

            return MapToPatientDto(link);
        }

        // ════════════════════════════════════════════════════════════
        // PATIENT: Revoke active connection
        // ════════════════════════════════════════════════════════════
        public async Task RevokeAccessAsync(Guid patientUserId, RevokeAccessDto request)
        {
            var link = await _uow.DoctorPatients
                .GetByIdWithDetailsAsync(request.DoctorPatientId)
                ?? throw new KeyNotFoundException("Connection not found.");

            if (link.Patient.UserId != patientUserId)
                throw new UnauthorizedAccessException(
                    "You can only revoke your own connections.");

            if (link.Status != ConnectionStatus.Active)
                throw new InvalidOperationException(
                    "Only active connections can be revoked.");

            link.Status = ConnectionStatus.Revoked;
            link.RevokedAt = DateTime.UtcNow;
            link.UpdatedBy = patientUserId;
            link.UpdatedAt = DateTime.UtcNow;

            _uow.DoctorPatients.Update(link);
            await _uow.SaveChangesAsync();

            if (link.DoctorId.HasValue)
            {
                await _notificationService.CreateAsync(
                    link.Doctor!.UserId,
                    NotificationType.Alert,
                    $"{link.Patient.User.FullName} revoked your access.");
            }
        }

        // ════════════════════════════════════════════════════════════
        // PATIENT: Get my doctors and requests
        // ════════════════════════════════════════════════════════════
        public async Task<IEnumerable<PatientDoctorResponseDto>> GetMyDoctorsAsync(
            Guid patientUserId)
        {
            var patient = await _uow.Patients
                .GetFirstOrDefaultAsync(p => p.UserId == patientUserId && !p.IsDeleted)
                ?? throw new KeyNotFoundException("Patient profile not found.");

            var links = await _uow.DoctorPatients.GetByPatientIdAsync(patient.Id);

            return links
                .Where(l => l.Status != ConnectionStatus.DeclinedDoctor &&
                            l.Status != ConnectionStatus.RejectedAdmin &&
                            l.Status != ConnectionStatus.Revoked)
                .Select(MapToPatientDto);
        }

        // ════════════════════════════════════════════════════════════
        // ADMIN: Approve request and assign doctor
        // ════════════════════════════════════════════════════════════
        public async Task<PatientDoctorResponseDto> ApproveRequestAsync(
            Guid adminUserId, AdminApproveRequestDto request)
        {
            var link = await _uow.DoctorPatients
                .GetByIdWithDetailsAsync(request.DoctorPatientId)
                ?? throw new KeyNotFoundException("Request not found.");

            if (link.Status != ConnectionStatus.PendingAdmin)
                throw new InvalidOperationException(
                    "Only pending_admin requests can be approved.");

            // Validate the assigned doctor
            var doctor = await _uow.Doctors
                .GetByIdWithDetailsAsync(request.DoctorId)
                ?? throw new KeyNotFoundException("Doctor not found.");

            if (!doctor.User.IsActive)
                throw new InvalidOperationException(
                    "Selected doctor is not active.");

            // Update link
            link.DoctorId = request.DoctorId;
            link.Status = ConnectionStatus.PendingDoctor;
            link.AdminReviewedAt = DateTime.UtcNow;
            link.ReviewedByAdminId = adminUserId;
            link.AdminNote = request.AdminNote;
            link.AssignedAt = DateTime.UtcNow;
            link.UpdatedBy = adminUserId;
            link.UpdatedAt = DateTime.UtcNow;

            _uow.DoctorPatients.Update(link);
            await _uow.SaveChangesAsync();

            // Reload with doctor navigation
            link = await _uow.DoctorPatients
                .GetByIdWithDetailsAsync(link.Id)
                ?? throw new Exception("Failed to reload.");

            // Notify patient — request approved
            await _notificationService.CreateAsync(
                link.Patient.UserId,
                NotificationType.Success,
                $"Your connection request has been approved. " +
                $"Dr. {doctor.User.FullName} will review it shortly.");

            // Notify doctor — new patient assigned
            await _notificationService.CreateAsync(
                doctor.UserId,
                NotificationType.Info,
                $"A new patient {link.Patient.User.FullName} has been assigned to you. " +
                $"Please accept or decline.");

            // Real-time push to doctor
            try
            {
                await _hubContext.Clients
                    .User(doctor.UserId.ToString())
                    .SendAsync("NewRequestIncoming", MapToDoctorDto(link));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalR] NewRequestIncoming failed: {ex.Message}");
            }

            return MapToPatientDto(link);
        }

        // ════════════════════════════════════════════════════════════
        // ADMIN: Reject request
        // ════════════════════════════════════════════════════════════
        public async Task RejectRequestAsync(Guid adminUserId, AdminRejectRequestDto request)
        {
            var link = await _uow.DoctorPatients
                .GetByIdWithDetailsAsync(request.DoctorPatientId)
                ?? throw new KeyNotFoundException("Request not found.");

            if (link.Status != ConnectionStatus.PendingAdmin)
                throw new InvalidOperationException(
                    "Only pending_admin requests can be rejected.");

            link.Status = ConnectionStatus.RejectedAdmin;
            link.AdminReviewedAt = DateTime.UtcNow;
            link.ReviewedByAdminId = adminUserId;
            link.AdminNote = request.AdminNote;
            link.UpdatedBy = adminUserId;
            link.UpdatedAt = DateTime.UtcNow;

            _uow.DoctorPatients.Update(link);
            await _uow.SaveChangesAsync();

            // Notify patient
            var note = string.IsNullOrWhiteSpace(request.AdminNote)
                ? "Your connection request was not approved at this time."
                : $"Your connection request was not approved: {request.AdminNote}";

            await _notificationService.CreateAsync(
                link.Patient.UserId, NotificationType.Alert, note);
        }

        // ════════════════════════════════════════════════════════════
        // ADMIN: Get pending requests (pending_admin only)
        // ════════════════════════════════════════════════════════════
        public async Task<IEnumerable<AdminConnectionRequestDto>> GetPendingAdminRequestsAsync()
        {
            var links = await _uow.DoctorPatients.GetPendingAdminRequestsAsync();
            return links.Select(MapToAdminDto);
        }

        // ════════════════════════════════════════════════════════════
        // ADMIN: Get all requests with filters
        // ════════════════════════════════════════════════════════════
        public async Task<IEnumerable<AdminConnectionRequestDto>> GetAllRequestsForAdminAsync(
            AdminDoctorPatientFilterDto filter)
        {
            var links = await _uow.DoctorPatients.GetAllForAdminAsync();
            var query = links.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.ToLower();
                query = query.Where(l =>
                    (l.Patient?.User?.FullName?.ToLower().Contains(s) ?? false) ||
                    (l.Doctor?.User?.FullName?.ToLower().Contains(s) ?? false));
            }

            if (filter.DoctorId.HasValue)
                query = query.Where(l => l.DoctorId == filter.DoctorId);

            if (filter.Status.HasValue)
            {
                query = query.Where(l => l.Status == filter.Status.Value);
            }

            return query
                .OrderByDescending(l => l.RequestedAt)
                .Select(MapToAdminDto)
                .ToList();
        }

        // ════════════════════════════════════════════════════════════
        // DOCTOR: Accept or decline (after admin approval)
        // ════════════════════════════════════════════════════════════
        public async Task<DoctorPatientResponseDto> RespondToRequestAsync(
            Guid doctorUserId, RespondToRequestDto request)
        {
            var link = await _uow.DoctorPatients
                .GetByIdWithDetailsAsync(request.DoctorPatientId)
                ?? throw new KeyNotFoundException("Request not found.");

            if (link.Doctor?.UserId != doctorUserId)
                throw new UnauthorizedAccessException(
                    "You can only respond to your own patient requests.");

            if (link.Status != ConnectionStatus.PendingDoctor)
                throw new InvalidOperationException(
                    "Only pending_doctor requests can be accepted or declined.");

            link.Status = request.Accept ? ConnectionStatus.Active : ConnectionStatus.DeclinedDoctor;
            link.UpdatedBy = doctorUserId;
            link.UpdatedAt = DateTime.UtcNow;

            if (request.Accept)
                link.AssignedAt = DateTime.UtcNow;

            _uow.DoctorPatients.Update(link);
            await _uow.SaveChangesAsync();

            // Notify patient
            var doctorName = link.Doctor?.User?.FullName ?? "Your doctor";
            var msg = request.Accept
                ? $"Dr. {doctorName} accepted your connection request. You are now connected."
                : $"Dr. {doctorName} declined your connection request.";

            await _notificationService.CreateAsync(
                link.Patient.UserId,
                request.Accept ? NotificationType.Success : NotificationType.Alert,
                msg);

            // Real-time push to patient
            try
            {
                await _hubContext.Clients
                    .User(link.Patient.UserId.ToString())
                    .SendAsync("DoctorRequestUpdated", new
                    {
                        doctorPatientId = link.Id.ToString(),
                        status = link.Status,
                        fullName = doctorName,
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalR] DoctorRequestUpdated failed: {ex.Message}");
            }

            return MapToDoctorDto(link);
        }

        // ════════════════════════════════════════════════════════════
        // DOCTOR: Get pending_doctor requests
        // ════════════════════════════════════════════════════════════
        public async Task<IEnumerable<DoctorPatientResponseDto>> GetPendingRequestsAsync(
            Guid doctorUserId)
        {
            var doctor = await _uow.Doctors.GetByUserIdAsync(doctorUserId)
                ?? throw new KeyNotFoundException("Doctor profile not found.");

            var links = await _uow.DoctorPatients
                .GetPendingByDoctorIdAsync(doctor.Id);

            return links.Select(MapToDoctorDto);
        }

        // ════════════════════════════════════════════════════════════
        // DOCTOR: Get my active patients
        // ════════════════════════════════════════════════════════════
        public async Task<IEnumerable<DoctorPatientResponseDto>> GetMyPatientsAsync(
            Guid doctorUserId)
        {
            var doctor = await _uow.Doctors.GetByUserIdAsync(doctorUserId)
                ?? throw new KeyNotFoundException("Doctor profile not found.");

            var links = await _uow.DoctorPatients.GetByDoctorIdAsync(doctor.Id);

            return links
                .Where(l => l.Status == ConnectionStatus.Active)
                .Select(MapToDoctorDto);
        }

        // ── Private helpers ───────────────────────────────────────────

        private async Task NotifyAdminsAsync(string message)
        {
            // Get all admin users and notify each
            var admins = await _uow.Admins.GetAllAsync();
            foreach (var admin in admins)
            {
                await _notificationService.CreateAsync(
                    admin.UserId, NotificationType.Info, message);
            }
        }

        private static PatientDoctorResponseDto MapToPatientDto(DoctorPatient link)
        {
            var doctor = link.Doctor;
            var user = doctor?.User;

            var totalPatients = doctor?.DoctorPatients?
                .Count(dp => dp.Status == ConnectionStatus.Active && !dp.IsDeleted) ?? 0;

            return new PatientDoctorResponseDto
            {
                DoctorPatientId = link.Id,
                PatientId = link.PatientId,
                DoctorId = link.DoctorId,
                FullName = user?.FullName,
                Specialization = doctor?.Specialization,
                Hospital = doctor?.Hospital,
                Email = user?.Email,
                ContactNumber = doctor?.ContactNumber,
                YearsExperience = doctor?.YearsExperience,
                TotalPatients = totalPatients,
                Status = link.Status switch
                {
                    ConnectionStatus.PendingAdmin => "pending_admin",
                    ConnectionStatus.PendingDoctor => "pending_doctor",
                    ConnectionStatus.DeclinedDoctor => "declined_doctor",
                    ConnectionStatus.RejectedAdmin => "rejected_admin",
                    // For single-word statuses like Active and Revoked, just make them lowercase
                    _ => link.Status.ToString().ToLower()
                },
                AdminNote = link.AdminNote,
                RequestedAt = link.RequestedAt,
                AssignedAt = link.AssignedAt,
            };
        }

        private static DoctorPatientResponseDto MapToDoctorDto(DoctorPatient link)
        {
            var patient = link.Patient;
            var user = patient?.User;

            int? age = patient?.DateOfBirth.HasValue == true
                ? (int)((DateTime.UtcNow -
                    patient.DateOfBirth!.Value.ToDateTime(TimeOnly.MinValue))
                    .TotalDays / 365.25)
                : null;

            return new DoctorPatientResponseDto
            {
                DoctorPatientId = link.Id,
                DoctorId = link.DoctorId ?? Guid.Empty,
                PatientId = link.PatientId,
                FullName = user?.FullName ?? string.Empty,
                Email = user?.Email ?? string.Empty,
                ContactNumber = patient?.ContactNumber,
                Gender = patient?.Gender?.ToString(),
                BloodGroup = patient?.BloodGroup,
                DateOfBirth = patient?.DateOfBirth,
                Age = age,
                Status = link.Status switch
                {
                    ConnectionStatus.PendingAdmin => "pending_admin",
                    ConnectionStatus.PendingDoctor => "pending_doctor",
                    ConnectionStatus.DeclinedDoctor => "declined_doctor",
                    ConnectionStatus.RejectedAdmin => "rejected_admin",
                    // For single-word statuses like Active and Revoked, just make them lowercase
                    _ => link.Status.ToString().ToLower()
                },
                RequestedAt = link.RequestedAt,
                AssignedAt = link.AssignedAt,
            };
        }

        private static AdminConnectionRequestDto MapToAdminDto(DoctorPatient link)
        {
            return new AdminConnectionRequestDto
            {
                DoctorPatientId = link.Id,
                RequestNumber = $"REQ-{link.Id.ToString()[..8].ToUpper()}",
                PatientId = link.PatientId,
                PatientName = link.Patient?.User?.FullName ?? string.Empty,
                DoctorId = link.DoctorId,
                DoctorName = link.Doctor?.User?.FullName,
                Specialization = link.Doctor?.Specialization,
                Status = link.Status switch
                {
                    ConnectionStatus.PendingAdmin => "pending_admin",
                    ConnectionStatus.PendingDoctor => "pending_doctor",
                    ConnectionStatus.DeclinedDoctor => "declined_doctor",
                    ConnectionStatus.RejectedAdmin => "rejected_admin",
                    // For single-word statuses like Active and Revoked, just make them lowercase
                    _ => link.Status.ToString().ToLower()
                },
                AdminNote = link.AdminNote,
                RequestedAt = link.RequestedAt,
                AdminReviewedAt = link.AdminReviewedAt,
            };
        }
        public async Task<AdminDoctorPatientOverviewDto> GetAdminOverviewAsync(AdminDoctorPatientFilterDto filter)
        {
            // 1. Establish an UNFILTERED reference query line to fetch global counts, cards, and dropdown lookups
            var baseQuery = _uow.DoctorPatients.GetAllWithDetailsQueryable();

            //  STEP A: STATIC METRICS SCORECARDS (Always computes true systemic counts) ──
            var globalStatuses = await baseQuery
                .Select(l => l.Status)
                .ToListAsync();

            int totalCount = globalStatuses.Count;
            int activeCount = globalStatuses.Count(s => s == ConnectionStatus.Active);
            int pendingCount = globalStatuses.Count(s => s == ConnectionStatus.PendingAdmin || s == ConnectionStatus.PendingDoctor);
            int revokedCount = globalStatuses.Count(s => s == ConnectionStatus.Revoked);

            //  STEP B: STATIC PHYSICIAN PROFILE GRIDS (Always shows all active doctors & care teams) ──
            var staticDoctorCardsQuery = baseQuery
                .Where(l => l.Status == ConnectionStatus.Active)
                .GroupBy(l => l.DoctorId)
                .Select(group => new AdminDoctorCardDto
                {
                    DoctorId = group.Key,
                    DoctorName = group.First().Doctor.User.FullName,
                    Specialization = group.First().Doctor.Specialization,
                    PatientCount = group.Count(),
                    Patients = group.Select(x => x.Patient.User.FullName)
                                    .Distinct()
                                    .Take(3)
                                    .ToList()
                });

            var staticDoctorCards = await staticDoctorCardsQuery.ToListAsync();

            //  STEP C: STATIC DROPDOWN LIST OPTION ARCHITECTURE ──
            // If your dropdown select relies on a separate "All Unique Doctors" list, we extract it cleanly here
            var staticDropdownSelectQuery = baseQuery
                .GroupBy(l => new { l.DoctorId, l.Doctor.User.FullName })
                .Select(g => new AdminDoctorCardDto // Re-using DTO or lightweight mapping interface safely
                {
                    DoctorId = g.Key.DoctorId,
                    DoctorName = g.Key.FullName
                });

            var staticDropdownDoctors = await staticDropdownSelectQuery.ToListAsync();


            // STEP D: APPLY FILTERS EXCLUSIVELY TO THE DATA TABLE ──
            // Create a separate query instance dedicated solely to slicing rows for the grid log
            var tableQuery = _uow.DoctorPatients.GetAllWithDetailsQueryable();

            // 1. Live Text Box Filtering
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower().Trim();
                tableQuery = tableQuery.Where(l =>
                    l.Patient.User.FullName.ToLower().Contains(search) ||
                    l.Doctor.User.FullName.ToLower().Contains(search));
            }

            // 2. Dropdown Doctor Selection Filter
            if (filter.DoctorId.HasValue && filter.DoctorId.Value != Guid.Empty)
            {
                tableQuery = tableQuery.Where(l => l.DoctorId == filter.DoctorId.Value);
            }

            // 3. Dropdown Status Filter
            if (filter.Status.HasValue)
            {
                tableQuery = tableQuery.Where(l => l.Status == filter.Status.Value);
            }

            // Compute subcount matches specifically for pagination totals matching active table filters
            int filteredTableTotal = await tableQuery.CountAsync();

            //  STEP E: SERVER-SIDE PAGINATION LIMITS SPLICING ──
            int pageNumber = filter.PageNumber > 0 ? filter.PageNumber : 1;
            int pageSize = filter.PageSize > 0 ? filter.PageSize : 7;

            var pagedRequestsList = await tableQuery
                .OrderByDescending(dp => dp.RequestedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(link => new AdminDoctorPatientTableDto
                {
                    DoctorPatientId = link.Id,
                    PatientName = link.Patient.User.FullName,
                    DoctorName = link.Doctor.User.FullName,
                    Specialization = link.Doctor.Specialization,
                    Status = link.Status.ToString(),
                    RequestedAt = link.RequestedAt
                })
                .ToListAsync();

            // STEP F: COMPILE AND PACK THE BALANCED STRUCTURAL DTO RENDER ──
            return new AdminDoctorPatientOverviewDto
            {
                // Global scorecards remain static and secure
                TotalConnections = totalCount,
                ActiveLinks = activeCount,
                PendingLinks = pendingCount,
                RevokedLinks = revokedCount,

                // Roster elements stay locked regardless of lower table queries
                Doctors = staticDoctorCards,

                // The table is dynamically updated with the search criteria results
                Requests = pagedRequestsList,

                PageNumber = pageNumber,
                PageSize = pageSize,

                // TotalPages uses the active filter subcount match so page navigation adjusts perfectly
                TotalPages = (int)Math.Ceiling((double)filteredTableTotal / pageSize)
            };
        }
    }
}