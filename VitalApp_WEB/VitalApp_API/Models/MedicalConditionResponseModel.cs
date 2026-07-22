namespace VitalApp_API.Models
{
    public class MedicalConditionResponseModel
    {
        public int MedicalConditionId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DiagnosticDate { get; set; }
    }
}
