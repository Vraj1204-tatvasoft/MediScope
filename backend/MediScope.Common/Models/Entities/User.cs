using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.Entities
{
    public class User : BaseEntity
    {
        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public UserRole Role { get; set; }

        public bool IsActive { get; set; } = true;
        public bool MustChangePassword { get; set; } = false;
        public Guid CurrentSessionId { get; set; } = Guid.NewGuid();
        // Navigation Properties
        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }
        public Admin? Admin { get; set; }
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}