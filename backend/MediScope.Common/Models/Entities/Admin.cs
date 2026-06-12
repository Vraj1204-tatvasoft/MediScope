namespace MediScope.Common.Models.Entities
{
    public class Admin : BaseEntity
    {
        public Guid UserId { get; set; }
        public string? Department { get; set; }
        public User User { get; set; } = null!;
    }
}