using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Pagination;
using MediScope.Data.Repositories;

namespace MediScope.Business.Services
{
    public class PatientAdmissionService : IPatientAdmissionService
    {
        private readonly IPatientAdmissionRepository _repository;

        public PatientAdmissionService(IPatientAdmissionRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> AdmitPatientAsync(AdmitPatientRequestDto request)
        {
            if (request.PatientId == Guid.Empty || request.BedId == Guid.Empty)
                throw new ArgumentException("Patient and Bed must be selected.");

            await _repository.AdmitPatientAsync(request);
            return true;
        }

        public async Task<bool> TransferPatientBedAsync(Guid admissionId, TransferBedRequestDto request)
        {
            if (request.NewBedId == Guid.Empty)
                throw new ArgumentException("A new bed must be selected for transfer.");

            await _repository.TransferPatientBedAsync(admissionId, request);
            return true;
        }

        public async Task<bool> DischargePatientAsync(Guid admissionId, DischargePatientRequestDto request)
        {
            await _repository.DischargePatientAsync(admissionId, request.DischargeNotes);
            return true;
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

            return true;
        }

        public async Task<List<RoomPatientDto>> GetActivePatientsByRoomAsync(Guid roomId)
        {
            if (roomId == Guid.Empty)
                throw new ArgumentException("Room Id is required.");

            return await _repository.GetActivePatientsByRoomAsync(roomId);
        }
    }
}