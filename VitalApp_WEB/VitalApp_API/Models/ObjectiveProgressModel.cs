namespace VitalApp_API.Models
{
    public class ObjectiveProgressRequestModel
    {
        public int ObjectiveId { get; set; }
        public DateTime Date { get; set; }
        public decimal CurrentValue { get; set; }
    }

    public class ObjectiveProgressResponseModel
    {
        public int ObjectiveProgressId { get; set; }
        public int ObjectiveId { get; set; }
        public DateTime Date { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal ComplianceRate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
