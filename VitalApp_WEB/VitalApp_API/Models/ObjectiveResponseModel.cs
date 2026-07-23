namespace VitalApp_API.Models
{
    public class ObjectiveResponseModel
    {
        public int ObjectiveId { get; set; }
        public int UserId { get; set; }
        public int IndicatorTypeId { get; set; }
        public string IndicatorTypeName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? InitialValue { get; set; }
        public decimal? ObjectiveValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? LimitDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? LastValue { get; set; }
        public decimal? ComplianceRate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
