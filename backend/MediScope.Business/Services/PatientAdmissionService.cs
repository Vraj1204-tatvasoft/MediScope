using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Pagination;
using MediScope.Data.Repositories;
using Microsoft.AspNetCore.SignalR;
using MediScope.Business.Hubs;
using MediScope.Common.Models.Entities;
using Npgsql;

namespace MediScope.Business.Services
{
    public class PatientAdmissionService : IPatientAdmissionService
    {
        private readonly IPatientAdmissionRepository _repository;
        private readonly IHubContext<RealtimeHub> _hubContext;
        public PatientAdmissionService(IPatientAdmissionRepository repository, IHubContext<RealtimeHub> hubContext)
        {
            _repository = repository;
            _hubContext = hubContext;
        }

        public async Task<bool> AdmitPatientAsync(AdmitPatientRequestDto request)
        {
            if (request.PatientId == Guid.Empty || request.BedId == Guid.Empty)
                throw new ArgumentException("Patient and Bed must be selected.");

            await _repository.AdmitPatientAsync(request);
            await _hubContext.Clients.All.SendAsync("DashboardUpdated");
            return true;
        }

        public async Task<bool> TransferPatientBedAsync(Guid admissionId, TransferBedRequestDto request)
        {
            if (request.NewBedId == Guid.Empty)
                throw new ArgumentException("A new bed must be selected for transfer.");

            await _repository.TransferPatientBedAsync(admissionId, request);
            await _hubContext.Clients.All.SendAsync("DashboardUpdated");
            return true;
        }

        public async Task<bool> DischargePatientAsync(Guid admissionId, DischargePatientRequestDto request)
        {
            try
            {
                await _repository.DischargePatientAsync(admissionId, request.DischargeNotes, request.DischargeDate);
                await _hubContext.Clients.All.SendAsync("DashboardUpdated");
                return true;
            }
            catch (PostgresException ex)
            {
                throw new ArgumentException(ex.MessageText);
            }
        }

        public async Task<PagedResult<AdmissionSummaryDto>> GetAdmissionsPagedAsync(PaginationParams request)
        {
            return await _repository.GetAdmissionsPagedAsync(request);
        }

        public async Task<AdmissionDetailsDto> GetAdmissionByIdAsync(Guid admissionId)
        {
            var admission = await _repository.GetAdmissionByIdAsync(admissionId);

            if (admission == null)
                throw new KeyNotFoundException("Admission not found.");

            return admission;
        }

        public async Task<bool> UpdateAdmissionAsync(
            Guid admissionId,
            UpdateAdmissionRequestDto request)
        {
            await _repository.UpdateAdmissionAsync(admissionId, request);
            await _hubContext.Clients.All.SendAsync("DashboardUpdated");
            return true;
        }

        public async Task<List<RoomPatientDto>> GetActivePatientsByRoomAsync(Guid roomId)
        {
            if (roomId == Guid.Empty)
                throw new ArgumentException("Room Id is required.");

            return await _repository.GetActivePatientsByRoomAsync(roomId);
        }

        public async Task CheckInPatientAsync(Guid admissionId)
        {
            await _repository.CheckInPatientAsync(admissionId);
        }

        public async Task CancelAdmissionAsync(Guid admissionId)
        {
            await _repository.CancelAdmissionAsync(admissionId);
        }

        public async Task<AvailableBedResponseDto?> GetFirstAvailableBedAsync(Guid roomId, DateTime start, DateTime end)
        {
            return await _repository.GetFirstAvailableBedAsync(roomId, start, end);
        }
    }
}