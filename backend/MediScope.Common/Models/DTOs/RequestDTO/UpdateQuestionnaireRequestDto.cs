using System.ComponentModel.DataAnnotations;

namespace MediScope.Common.Models.DTOs.Request
{
    public class UpdateQuestionnaireRequestDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(255, ErrorMessage = "Name must not exceed 255 characters.")]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        [MaxLength(150, ErrorMessage = "Department must not exceed 150 characters.")]
        public string? Department { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression("^(Active|Inactive)$", ErrorMessage = "Status must be Active or Inactive.")]
        public string Status { get; set; } = "Active";
    }
}