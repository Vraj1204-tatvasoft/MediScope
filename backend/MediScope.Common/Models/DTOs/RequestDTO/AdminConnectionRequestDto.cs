public class AdminConnectionRequestDto
{
    public Guid DoctorPatientId { get; set; }
    public string RequestNumber { get; set; } = null!;
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = null!;
    public Guid? DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public string? Specialization { get; set; }
    public string Status { get; set; } = null!;
    public string? AdminNote { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? AdminReviewedAt { get; set; }
}