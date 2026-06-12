namespace MediScope.Common.Models.DTOs.Request
{
    public class AdminDoctorPatientFilterDto
    {
        public string? Search { get; set; }
        public Guid? DoctorId { get; set; }
        public string? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 7;
    }
}