namespace MediScope.Common.Models.DTOs.Request
{
    public class UpdateWardRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}