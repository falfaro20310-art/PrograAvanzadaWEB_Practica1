namespace VitalApp_API.Models
{
    public class UpdateObjectiveRequestModel
    {
        public int ObjectiveId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? ObjectiveValue { get; set; }
        public DateTime? LimitDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}