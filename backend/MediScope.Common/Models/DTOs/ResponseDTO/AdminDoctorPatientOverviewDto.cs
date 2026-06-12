using MediScope.Common.Models.Pagination;
using MediScope.Common.Models.DTOs.Request;
namespace MediScope.Common.Models.DTOs.Response
{
    public class AdminDoctorPatientOverviewDto
    {
        public int TotalConnections { get; set; }
        public int ActiveLinks { get; set; }
        public int PendingLinks { get; set; }
        public int RevokedLinks { get; set; }
        public IEnumerable<AdminDoctorCardDto> Doctors { get; set; } = new List<AdminDoctorCardDto>();
        public IEnumerable<AdminDoctorPatientTableDto> Requests { get; set; } = new List<AdminDoctorPatientTableDto>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class AdminDoctorCardDto
    {
        public Guid? DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;
        public string? Specialization { get; set; }
        public int PatientCount { get; set; }
        public List<string> Patients { get; set; } = new();
    }

    public class AdminDoctorPatientTableDto
    {
        public Guid DoctorPatientId { get; set; }
        public string PatientName { get; set; } = null!;
        public string DoctorName { get; set; } = null!;
        public string? Specialization { get; set; }
        public string Status { get; set; } = null!;
        public DateTime RequestedAt { get; set; }

    }
}