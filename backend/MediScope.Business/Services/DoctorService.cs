// File: MediScope.Business/Services/DoctorService.cs

using MediScope.Business.Helpers;
using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Entities;
using MediScope.Common.Models.Enums;
using MediScope.Data.Repositories;

namespace MediScope.Business.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;
        private readonly IEmailService _emailService;

        public DoctorService(
            IUnitOfWork uow,
            ICurrentUserService currentUser,
            IEmailService emailService)
        {
            _uow = uow;
            _currentUser = currentUser;
            _emailService = emailService;
        }

        // ── CREATE — Admin only
        public async Task<DoctorResponseDto> CreateDoctorAsync(CreateDoctorRequestDto request)
        {
            // 1. Check email uniqueness
            var emailExists = await _uow.Users.AnyAsync(
                u => u.Email == request.Email.ToLower());

            if (emailExists)
                throw new InvalidOperationException("A user with this email already exists.");

            // 2. Check license number uniqueness
            var licenseExists = await _uow.Doctors.AnyAsync(
                d => d.LicenseNumber == request.LicenseNumber);

            // 3. Check contact number uniqueness
            if (!string.IsNullOrWhiteSpace(request.ContactNumber))
            {
                var contactExists = await _uow.Doctors.AnyAsync(
                    d => d.ContactNumber == request.ContactNumber);

                if (contactExists)
                {
                    throw new InvalidOperationException(
                        "A doctor with this contact number already exists.");
                }
            }
            if (licenseExists)
                throw new InvalidOperationException("A doctor with this license number already exists.");

            // 3. Generate temporary password
            var tempPassword = $"Doc@{Random.Shared.Next(100000, 999999)}";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(tempPassword);

            // 4. Create user
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email.ToLower(),
                PasswordHash = hashedPassword,
                Role = UserRole.Doctor,
                IsActive = true,
                MustChangePassword = true
            };

            await _uow.BeginTransactionAsync();
            try
            {
                // 5. Save user first (FK requirement)
                await _uow.Users.AddAsync(user);
                await _uow.SaveChangesAsync();

                user.CreatedBy = _currentUser.UserId;
                user.UpdatedBy = _currentUser.UserId;
                _uow.Users.Update(user);

                // 6. Create doctor profile
                var doctor = new Doctor
                {
                    UserId = user.Id,
                    Specialization = request.Specialization,
                    LicenseNumber = request.LicenseNumber,
                    ContactNumber = request.ContactNumber,
                    Hospital = request.Hospital,
                    YearsExperience = request.YearsExperience,
                    Bio = request.Bio,
                    CreatedBy = _currentUser.UserId,
                    UpdatedBy = _currentUser.UserId,
                };

                await _uow.Doctors.AddAsync(doctor);
                await _uow.SaveChangesAsync();
                await _uow.CommitTransactionAsync();

                // 7. Send welcome email 
                try
                {
                    await _emailService.SendAsync(
                        to: request.Email,
                        subject: "Welcome to MediScope — Your Doctor Account",
                        body: EmailTemplates.DoctorWelcome(
                                      doctorName: request.FullName,
                                      email: request.Email,
                                      temporaryPassword: tempPassword)
                    );
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"[EmailService] Failed: {emailEx.Message}");
                }

                doctor.User = user;
                return await MapToDtoAsync(doctor);
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                throw;
            }
        }

        // ── GET BY ID — all roles 
        public async Task<DoctorResponseDto> GetDoctorByIdAsync(Guid doctorId)
        {
            var doctor = await _uow.Doctors.GetByIdWithDetailsAsync(doctorId)
                ?? throw new KeyNotFoundException("Doctor not found.");

            return await MapToDtoAsync(doctor);
        }

        // ── GET MY PROFILE — doctor only (by userId from JWT) ─────────
        public async Task<DoctorResponseDto> GetMyProfileAsync(Guid userId)
        {
            var doctor = await _uow.Doctors.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Doctor profile not found.");

            return await MapToDtoAsync(doctor);
        }

        // ── GET ALL — admin and patient 
        public async Task<IEnumerable<DoctorResponseDto>> GetAllDoctorsAsync()
        {
            var doctors = await _uow.Doctors.GetAllWithUserAsync();
            var result = new List<DoctorResponseDto>();

            foreach (var d in doctors)
                result.Add(await MapToDtoAsync(d));

            return result;
        }

        // ── UPDATE — doctor updates own profile 
        public async Task<DoctorResponseDto> UpdateMyProfileAsync(
            Guid userId, UpdateDoctorRequestDto request)
        {
            // 1. Load doctor with user
            var doctor = await _uow.Doctors.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Doctor profile not found.");

            var user = doctor.User;
            // 3. Check contact number uniqueness
            if (!string.IsNullOrWhiteSpace(request.ContactNumber))
            {
                var contactExists = await _uow.Doctors.AnyAsync(
                    d =>
                        d.ContactNumber == request.ContactNumber
                        && d.Id != doctor.Id);

                if (contactExists)
                {
                    throw new InvalidOperationException(
                        "Another doctor already uses this contact number.");
                }
            }
            // 2. Apply changes to User (name only — email is admin-controlled)
            user.FullName = request.FullName;
            user.UpdatedBy = userId;
            user.UpdatedAt = DateTime.UtcNow;

            // 3. Apply changes to Doctor
            doctor.ContactNumber = request.ContactNumber;
            doctor.Specialization = request.Specialization;
            doctor.Hospital = request.Hospital;
            doctor.YearsExperience = request.YearsExperience;
            doctor.Bio = request.Bio;
            doctor.UpdatedBy = userId;
            doctor.UpdatedAt = DateTime.UtcNow;

            // 4. Save in transaction
            await _uow.BeginTransactionAsync();
            try
            {
                _uow.Users.Update(user);
                _uow.Doctors.Update(doctor);
                await _uow.SaveChangesAsync();
                await _uow.CommitTransactionAsync();
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                throw;
            }

            return await MapToDtoAsync(doctor);
        }
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
            user.MustChangePassword = false;
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


        // ── Private helper 
        private async Task<DoctorResponseDto> MapToDtoAsync(Doctor doctor)
        {
            var patientCount = await _uow.Doctors
                .GetAssignedPatientCountAsync(doctor.Id);

            return new DoctorResponseDto
            {
                DoctorId = doctor.Id,
                UserId = doctor.UserId,
                FullName = doctor.User?.FullName ?? string.Empty,
                Email = doctor.User?.Email ?? string.Empty,
                ContactNumber = doctor.ContactNumber,
                Specialization = doctor.Specialization,
                LicenseNumber = doctor.LicenseNumber,
                Hospital = doctor.Hospital,
                YearsExperience = doctor.YearsExperience,
                Bio = doctor.Bio,
                IsActive = doctor.User?.IsActive ?? false,
                AssignedPatients = patientCount,
                RegisteredAt = doctor.CreatedAt,
            };
        }
    }
}