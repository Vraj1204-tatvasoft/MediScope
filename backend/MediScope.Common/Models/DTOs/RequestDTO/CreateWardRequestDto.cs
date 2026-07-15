namespace MediScope.Common.Models.DTOs.Request
{
    public class CreateWardRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}