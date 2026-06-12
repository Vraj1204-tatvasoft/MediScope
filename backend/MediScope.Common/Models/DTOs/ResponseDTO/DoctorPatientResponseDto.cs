public class DoctorPatientResponseDto
{
    public Guid DoctorPatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? ContactNumber { get; set; }
    public string? Gender { get; set; }
    public string? BloodGroup { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public int? Age { get; set; }
    public string Status { get; set; } = null!;
    public DateTime RequestedAt { get; set; }
    public DateTime? AssignedAt { get; set; }
}