namespace MediScope.Common.Models.DTOs.Response
{
    public class WardSummaryResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public long Total_Count { get; set; }
    }
}