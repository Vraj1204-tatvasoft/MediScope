using System.ComponentModel.DataAnnotations;

namespace MediScope.Common.Models.DTOs.Request
{
    public class AssignQuestionnaireRequestDto
    {
        [Required(ErrorMessage = "Questionnaire is required.")]
        public Guid QuestionnaireId { get; set; }

        [Required(ErrorMessage = "Patient is required.")]
        public Guid PatientId { get; set; }
        public string? Notes { get; set; }
    }
}