namespace MediScope.Common.Models.DTOs.Response
{
    public class AvailableBedResponseDto
    {
        public Guid Id { get; set; }
        public string BedNumber { get; set; } = string.Empty;
    }
}