using System.ComponentModel.DataAnnotations;

namespace VitalApp_API.Models
{
    public class CreateConsultationRequestModel
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Contexto opcional: una medicion que originó la consulta
        public int? MeasureId { get; set; }
    }
}
