using System.ComponentModel.DataAnnotations;

namespace VitalApp_API.Models
{
    public class MedicalConditionRequestModel
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        public DateTime DiagnosticDate { get; set; }
    }
}
