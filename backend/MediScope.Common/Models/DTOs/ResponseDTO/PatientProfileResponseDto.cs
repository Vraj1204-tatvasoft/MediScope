using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.DTOs.Response
{
    public class PatientProfileResponseDto
    {
        public Guid UserId { get; set; }

        public Guid PatientId { get; set; }

        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? ContactNumber { get; set; }

        public string? BloodGroup { get; set; }

        public Gender? Gender { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        public string? Address { get; set; }

        public bool ConsentProfileVisible { get; set; }

        public DateTime RegisteredAt { get; set; }

        public int? Age => DateOfBirth.HasValue
            ? (int)((DateTime.UtcNow -
                DateOfBirth.Value.ToDateTime(TimeOnly.MinValue))
                .TotalDays / 365.25)
            : null;
    }
}