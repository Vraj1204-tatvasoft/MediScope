namespace MediScope.Common.Models.Entities
{
    public class PasswordResetToken : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public User User { get; set; } = null!;
    }
}