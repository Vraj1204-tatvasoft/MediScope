using System.Text.Json;
using System.Text.Json.Serialization;
using MediScope.Common.Models.Enums;
namespace MediScope.Common.Models.DTOs.Response
{
    public class DbPagedBroadcast
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public BroadcastChannel Channel { get; set; }
        public int BatchSize { get; set; }
        public string? Subject { get; set; }
        public BroadcastAudience Audience { get; set; }
        public BroadcastStatus Status { get; set; }
        public int TotalRecipients { get; set; }
        public int SentCount { get; set; }
        public int FailedCount { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public int TotalCount { get; set; }
    }
    public class DbAudienceMember
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}