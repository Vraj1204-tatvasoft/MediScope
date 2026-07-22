namespace MediScope.Common.Models.DTOs.ResponseDTO
{
    public class HospitalizationSummaryDto
    {
        public int TotalRooms { get; set; }
        public int TotalBeds { get; set; }
        public int OccupiedBeds { get; set; }
        public int AvailableBeds { get; set; }
        public int AdmittedPatients { get; set; }
        public int DischargedPatientsToday { get; set; }
    }
}