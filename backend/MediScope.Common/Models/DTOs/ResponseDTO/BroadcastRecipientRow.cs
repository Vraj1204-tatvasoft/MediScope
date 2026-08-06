namespace MediScope.Common.Models.DTOs.Response
{
    public class BroadcastRecipientRow
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int RetryCount { get; set; }
        public int BatchNumber { get; set; }
    }
}