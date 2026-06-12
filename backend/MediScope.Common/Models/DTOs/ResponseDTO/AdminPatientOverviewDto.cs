using MediScope.Common.Models.Pagination;

namespace MediScope.Common.Models.DTOs.Response
{
    public class AdminPatientOverviewDto
    {
        public int TotalPatients { get; set; }
        public int MalePatients { get; set; }
        public int FemalePatients { get; set; }
        public int CriticalPatients { get; set; }
        public PagedResult<AdminPatientListItemDto> Patients { get; set; } = new();
    }
}