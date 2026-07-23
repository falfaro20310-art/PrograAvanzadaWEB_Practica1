namespace VitalApp_API.Models
{
    public class ObjectiveRequestModel
    {
        public int UserId { get; set; }
        public int IndicatorTypeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? InitialValue { get; set; }
        public decimal? ObjectiveValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? LimitDate { get; set; }
    }
}
