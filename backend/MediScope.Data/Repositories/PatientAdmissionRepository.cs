using Microsoft.EntityFrameworkCore;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Pagination;
using MediScope.Common.Models.Entities;
using MediScope.Common.Models.Enums;
using MediScope.Data;

namespace MediScope.Data.Repositories
{
    public class PatientAdmissionRepository : IPatientAdmissionRepository
    {
        private readonly AppDbContext _context;

        public PatientAdmissionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AdmitPatientAsync(AdmitPatientRequestDto request)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                CALL sp_admit_patient(
                    {request.PatientId}, 
                    {request.DoctorId}, 
                    {request.WardId}, 
                    {request.RoomId}, 
                    {request.BedId}, 
                    {request.AdmissionReason}, 
                    {request.AdmissionDate}, 
                    {request.ExpectedDischargeDate}, 
                    {request.Remarks}
                )");
        }

        public async Task TransferPatientBedAsync(Guid admissionId, TransferBedRequestDto request)
        {
            var sql = "CALL sp_transfer_patient_bed(@p0, @p1, @p2, @p3, @p4)";
            await _context.Database.ExecuteSqlRawAsync(sql,
                admissionId, request.NewWardId, request.NewRoomId, request.NewBedId, request.TransferReason);
        }

        public async Task DischargePatientAsync(Guid admissionId, string dischargeNotes, DateTime dischargeDate)
        {
            var sql = "CALL sp_discharge_patient(@p0, @p1, @p2)";
            await _context.Database.ExecuteSqlRawAsync(sql, admissionId, dischargeNotes, dischargeDate);
        }

        public async Task<PagedResult<AdmissionSummaryDto>> GetAdmissionsPagedAsync(PaginationParams request)
        {
            int? parsedStatus = int.TryParse(request.Status, out var s) ? s : null;

            var sql = "SELECT * FROM fn_get_admissions_paged(@p0, @p1, @p2, @p3)";

            var dbResults = await _context.Database.SqlQueryRaw<DbPagedAdmission>(
                sql,
                request.Search ?? (object)DBNull.Value,
                parsedStatus ?? (object)DBNull.Value,
                request.PageNumber,
                request.PageSize
            ).ToListAsync();

            var items = dbResults.Select(a => new AdmissionSummaryDto
            {
                Id = a.Id,
                AdmissionNumber = a.Admission_Number,
                PatientName = a.Patient_Name,
                DoctorName = a.Doctor_Name,
                WardName = a.Ward_Name,
                RoomNumber = a.Room_Number,
                BedNumber = a.Bed_Number,
                AdmissionDate = a.Admission_Date,
                Status = a.Status
            }).ToList();

            return new PagedResult<AdmissionSummaryDto>
            {
                Items = items,
                TotalCount = (int)(dbResults.FirstOrDefault()?.Total_Count ?? 0),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
        public async Task UpdateAdmissionAsync(Guid admissionId, UpdateAdmissionRequestDto request)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                CALL sp_update_admission(
                    {admissionId},
                    {request.DoctorId},
                    {request.AdmissionReason},
                    {request.AdmissionDate},
                    {request.ExpectedDischargeDate},
                    {request.Remarks}
                )");
        }
        public async Task<AdmissionDetailsDto?> GetAdmissionByIdAsync(Guid admissionId)
        {
            return await _context.Database
                .SqlQuery<AdmissionDetailsDto>($@"
                    SELECT *
                    FROM fn_get_admission_by_id({admissionId})
                ")
                .FirstOrDefaultAsync();
        }
        public async Task<List<RoomPatientDto>> GetActivePatientsByRoomAsync(Guid roomId)
        {
            return await _context.Database
                .SqlQuery<RoomPatientDto>($@"
                    SELECT *
                    FROM fn_get_room_active_patients({roomId})
                ")
                .ToListAsync();
        }
        public async Task CheckInPatientAsync(Guid admissionId)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($"CALL sp_checkin_patient({admissionId})");
        }
        public async Task CancelAdmissionAsync(Guid admissionId)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($"CALL sp_cancel_admission({admissionId})");
        }
        public async Task<AvailableBedResponseDto?> GetFirstAvailableBedAsync(Guid roomId, DateTime start, DateTime end)
        {
            var query = @"SELECT id, bed_number FROM fn_get_first_available_bed(@p0, @p1, @p2)";

            var availableBed = await _context.Database
                .SqlQueryRaw<AvailableBedResponseDto>(query, roomId, start, end)
                .FirstOrDefaultAsync();

            return availableBed;
        }
    }
}