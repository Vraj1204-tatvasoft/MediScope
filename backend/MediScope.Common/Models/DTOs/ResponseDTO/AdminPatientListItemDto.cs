namespace MediScope.Common.Models.DTOs.Response
{
    public class AdminPatientListItemDto
    {
        public Guid PatientId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int? Age { get; set; }
        public string? Gender { get; set; }
        public string? BloodGroup { get; set; }
        public IEnumerable<string> Doctors { get; set; } = new List<string>();
        public int TotalRecords { get; set; }
        public string LatestStatus { get; set; } = "Normal";
    }
}