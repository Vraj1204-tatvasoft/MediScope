namespace MediScope.Common.Models.DTOs.Response
{
    public class DoctorResponseDto
    {
        public Guid DoctorId { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? ContactNumber { get; set; }
        public string? Specialization { get; set; }
        public string LicenseNumber { get; set; } = null!;
        public string? Hospital { get; set; }
        public int? YearsExperience { get; set; }
        public string? Bio { get; set; }
        public bool IsActive { get; set; }
        public int AssignedPatients { get; set; }
        public DateTime RegisteredAt { get; set; }
    }
}