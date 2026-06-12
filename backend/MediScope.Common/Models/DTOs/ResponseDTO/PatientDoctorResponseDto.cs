namespace MediScope.Common.Models.DTOs.Response
{
    public class PatientDoctorResponseDto
    {
        public Guid DoctorPatientId { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid PatientId { get; set; }
        public string? FullName { get; set; } = null!;
        public string? Specialization { get; set; }
        public string? Hospital { get; set; }
        public string Email { get; set; } = null!;
        public string? ContactNumber { get; set; }
        public int? YearsExperience { get; set; }
        public int TotalPatients { get; set; }
        public string Status { get; set; } = null!;
        public string? AdminNote { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? AssignedAt { get; set; }
    }
}