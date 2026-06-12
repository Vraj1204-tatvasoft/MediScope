using MediScope.Common.Models.Enums;
public class PatientProfileDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateOnly? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public string? BloodGroup { get; set; }
    public string? ContactNumber { get; set; }
    public string? Address { get; set; }
    public bool ConsentProfileVisible { get; set; }
}