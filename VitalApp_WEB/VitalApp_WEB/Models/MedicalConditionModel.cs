namespace VitalApp_WEB.Models
{
    // Condicion medica asociada al usuario
    public class MedicalConditionModel
    {
        public int MedicalConditionId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DiagnosticDate { get; set; }
    }
}
