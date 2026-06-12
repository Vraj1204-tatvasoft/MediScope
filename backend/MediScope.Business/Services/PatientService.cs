// File: MediScope.Business/Services/PatientService.cs

using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Entities;
using MediScope.Data.Repositories;
using MediScope.Common.Models.Pagination;

namespace MediScope.Business.Services
{
    public class PatientService
        : GenericService<Patient, PatientProfileResponseDto, UpdateProfileRequestDto, UpdateProfileRequestDto>,
          IPatientService
    {
        public PatientService(IUnitOfWork uow, ICurrentUserService currentUser)
            : base(uow, currentUser) { }

        // ── Required by GenericService ────────────────────────────────

        /// <summary>Points GenericService to the Patients repository in UnitOfWork</summary>
        protected override IGenericRepository<Patient> GetRepository()
            => _uow.Patients;

        /// <summary>Patient → PatientProfileResponseDto</summary>
        protected override PatientProfileResponseDto MapToResponseDto(Patient entity)
        {
            // User must be loaded — call GetFirstOrDefaultAsync with include
            return new PatientProfileResponseDto
            {
                UserId = entity.UserId,
                PatientId = entity.Id,
                FullName = entity.User?.FullName ?? string.Empty,
                Email = entity.User?.Email ?? string.Empty,
                ContactNumber = entity.ContactNumber,
                BloodGroup = entity.BloodGroup,
                Gender = entity.Gender,
                DateOfBirth = entity.DateOfBirth,
                Address = entity.Address,
                ConsentProfileVisible = entity.ConsentProfileVisible,
                RegisteredAt = entity.CreatedAt,
            };
        }

        /// <summary>UpdateProfileRequestDto → new Patient entity (used in generic CreateAsync)</summary>
        protected override Patient MapToEntity(UpdateProfileRequestDto dto)
        {
            // Patient creation is handled in AuthService (register flow)
            // This method exists to satisfy the generic contract
            throw new InvalidOperationException(
                "Patient creation is handled during registration. Use AuthService.RegisterAsync.");
        }

        /// <summary>Apply update DTO to existing Patient entity (used in generic UpdateAsync)</summary>
        protected override void ApplyUpdate(Patient entity, UpdateProfileRequestDto dto)
        {
            entity.ContactNumber = dto.ContactNumber;
            entity.BloodGroup = dto.BloodGroup;
            entity.Gender = dto.Gender;
            entity.DateOfBirth = dto.DateOfBirth;
            entity.Address = dto.Address;
        }

        // ── Patient-specific methods ──────────────────────────────────

        /// <summary>
        /// Get profile by userId (from JWT claims) — includes User navigation property.
        /// </summary>
        public async Task<PatientProfileResponseDto> GetMyProfileAsync(Guid userId)
        {
            var patient = await _uow.Patients.GetFirstOrDefaultAsync(
                p => p.UserId == userId && !p.IsDeleted,
                p => p.User
            ) ?? throw new KeyNotFoundException("Patient profile not found.");

            return MapToResponseDto(patient);
        }

        /// <summary>
        /// Update profile — handles user + patient tables + audit log in one transaction.
        /// </summary>
        public async Task<PatientProfileResponseDto> UpdateMyProfileAsync(
            Guid userId, UpdateProfileRequestDto request)
        {
            // 1. Load patient with user
            var patient = await _uow.Patients.GetFirstOrDefaultAsync(
                p => p.UserId == userId && !p.IsDeleted,
                p => p.User
            ) ?? throw new KeyNotFoundException("Patient profile not found.");

            var user = patient.User;

            // 2. Check email uniqueness if changed
            if (!string.Equals(user.Email, request.Email?.ToLower(), StringComparison.OrdinalIgnoreCase))
            {
                var emailTaken = await _uow.Users.AnyAsync(
                    u => u.Email == request.Email!.ToLower() && u.Id != userId);

                if (emailTaken)
                    throw new InvalidOperationException("This email address is already in use.");
            }

            // 3. Build audit entries before applying changes
            var auditEntries = BuildAuditEntries(patient, user, request, userId);

            // 4. Apply changes to User
            user.FullName = request.FullName;
            user.Email = request.Email!.ToLower();
            user.UpdatedBy = userId;
            user.UpdatedAt = DateTime.UtcNow;

            // 5. Apply changes to Patient
            ApplyUpdate(patient, request);
            patient.UpdatedBy = userId;
            patient.UpdatedAt = DateTime.UtcNow;

            // 6. Save in transaction
            await _uow.BeginTransactionAsync();
            try
            {
                _uow.Users.Update(user);
                _uow.Patients.Update(patient);

                foreach (var entry in auditEntries)
                    await _uow.PatientAuditLogs.AddAsync(entry);

                await _uow.SaveChangesAsync();
                await _uow.CommitTransactionAsync();
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                throw;
            }

            return MapToResponseDto(patient);
        }

        /// <summary>
        /// Change password — verifies current, prevents reuse, revokes all refresh tokens.
        /// </summary>
        public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request)
        {
            // 1. Get user
            var user = await _uow.Users.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            // 2. Verify current password
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                throw new InvalidOperationException("Current password is incorrect.");

            // 3. Prevent reuse
            if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.PasswordHash))
                throw new InvalidOperationException("New password must be different from the current password.");

            // 4. Hash and save
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedBy = userId;
            user.UpdatedAt = DateTime.UtcNow;

            // 5. Revoke all active refresh tokens → force re-login on all devices
            var activeTokens = await _uow.RefreshTokens.FindAsync(
                rt => rt.UserId == userId && !rt.IsRevoked);

            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                _uow.RefreshTokens.Update(token);
            }

            _uow.Users.Update(user);
            await _uow.SaveChangesAsync();
        }
        public async Task<AdminPatientOverviewDto> GetAdminPatientsAsync(
        AdminPatientFilterDto filter,
        PaginationParams pagination)
        {
            // USE DEDICATED PATIENT REPOSITORY

            var patients = await _uow.Patients
                .GetAllAdminPatientsAsync();

            // SEARCH FILTER

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();

                patients = patients.Where(p =>
                    p.User.FullName.ToLower().Contains(search) ||
                    p.User.Email.ToLower().Contains(search) ||
                    (p.Address != null &&
                     p.Address.ToLower().Contains(search)));
            }

            // GENDER FILTER

            if (!string.IsNullOrWhiteSpace(filter.Gender))
            {
                patients = patients.Where(p =>
                    p.Gender != null &&
                    p.Gender.ToString()!.ToLower() ==
                    filter.Gender.ToLower());
            }

            var materialized = patients.ToList();

            // TOP SUMMARY CARDS

            var total = materialized.Count;

            var male = materialized.Count(p =>
                p.Gender != null &&
                p.Gender.ToString() == "Male");

            var female = materialized.Count(p =>
                p.Gender != null &&
                p.Gender.ToString() == "Female");

            // CRITICAL PATIENTS

            var critical = materialized.Count(p =>
            {
                var latestBatchId = p.HealthMetrics
                    .OrderByDescending(m => m.RecordedAt)
                    .Select(m => (Guid?)m.SubmissionId)
                    .FirstOrDefault();

                if (latestBatchId == null)
                    return false;

                // Grab all metrics that belong to that latest check-in
                var latestMetrics = p.HealthMetrics.Where(m => m.SubmissionId == latestBatchId.Value);

                return latestMetrics.Any(m =>
                    m.MetricDefinition != null &&
                    (
                        m.Value > (m.MetricDefinition.NormalMax ?? decimal.MaxValue) ||
                        m.Value < (m.MetricDefinition.NormalMin ?? decimal.MinValue)
                    ));
            });

            // TABLE ROWS

            var rows = materialized.Select(p =>
            {
                var age = p.DateOfBirth.HasValue
                    ? DateTime.UtcNow.Year - p.DateOfBirth.Value.Year
                    : (int?)null;

                var latestBatchId = p.HealthMetrics
                    .OrderByDescending(m => m.RecordedAt)
                    .Select(m => (Guid?)m.SubmissionId)
                    .FirstOrDefault();

                var latestMetrics = latestBatchId.HasValue
                    ? p.HealthMetrics.Where(m => m.SubmissionId == latestBatchId.Value).ToList()
                    : new List<HealthMetric>();

                // STATUS
                var latestStatus = latestMetrics.Any(m =>
                        m.MetricDefinition != null &&
                        (
                            m.Value > (m.MetricDefinition.NormalMax ?? decimal.MaxValue) ||
                            m.Value < (m.MetricDefinition.NormalMin ?? decimal.MinValue)
                        ))
                    ? "Critical"

                    : latestMetrics.Any(m =>
                            m.MetricDefinition != null &&
                            m.Value > ((m.MetricDefinition.NormalMax ?? decimal.MaxValue) * 0.9m))
                        ? "Warning"
                        : "Normal";

                var totalEncounters = p.HealthMetrics
                    .Select(m => m.SubmissionId)
                    .Distinct()
                    .Count();

                return new AdminPatientListItemDto
                {
                    PatientId = p.Id,
                    FullName = p.User.FullName,
                    Email = p.User.Email,
                    Age = age,
                    Gender = p.Gender?.ToString(),
                    BloodGroup = p.BloodGroup,
                    Doctors = p.DoctorPatients
                        .Where(dp =>
                            dp.Status == "active" &&
                            dp.Doctor != null &&
                            dp.Doctor.User != null)
                        .Select(dp =>
                            dp.Doctor.User.FullName)
                        .Distinct()
                        .DefaultIfEmpty("—"),
                    TotalRecords = totalEncounters,
                    LatestStatus = latestStatus
                };
            }).ToList();

            // PAGINATION

            var totalRows = rows.Count;

            var pagedItems = rows
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();

            return new AdminPatientOverviewDto
            {
                TotalPatients = total,

                MalePatients = male,

                FemalePatients = female,

                CriticalPatients = critical,

                Patients = new PagedResult<AdminPatientListItemDto>
                {
                    Items = pagedItems,
                    TotalCount = totalRows,
                    PageNumber = pagination.PageNumber,
                    PageSize = pagination.PageSize
                }
            };
        }
        // ── Private Helpers ───────────────────────────────────────────

        private static List<PatientAuditLog> BuildAuditEntries(
            Patient patient, User user,
            UpdateProfileRequestDto request, Guid changedByUserId)
        {
            var entries = new List<PatientAuditLog>();
            var now = DateTime.UtcNow;

            void Track(string field, string? oldVal, string? newVal)
            {
                if (string.Equals(oldVal, newVal, StringComparison.Ordinal)) return;
                entries.Add(new PatientAuditLog
                {
                    PatientId = patient.Id,
                    ChangedByUserId = changedByUserId,
                    FieldName = field,
                    OldValue = oldVal,
                    NewValue = newVal,
                    ChangedAt = now,
                    CreatedBy = changedByUserId
                });
            }

            Track("FullName", user.FullName, request.FullName);
            Track("Email", user.Email, request.Email?.ToLower());
            Track("ContactNumber", patient.ContactNumber, request.ContactNumber);
            Track("BloodGroup", patient.BloodGroup, request.BloodGroup);
            Track("Gender", patient.Gender?.ToString(), request.Gender?.ToString());
            Track("DateOfBirth", patient.DateOfBirth?.ToString(), request.DateOfBirth?.ToString());
            Track("Address", patient.Address, request.Address);

            return entries;
        }
    }
}