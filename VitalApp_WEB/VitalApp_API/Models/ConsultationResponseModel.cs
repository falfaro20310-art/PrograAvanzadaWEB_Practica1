namespace VitalApp_API.Models
{
    public class ConsultationResponseModel
    {
        public int ConsultationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string InterlocutorName { get; set; } = string.Empty;

        // Contexto opcional: medicion adjunta
        public int? MeasureId { get; set; }
        public string MeasureIndicator { get; set; } = string.Empty;
        public decimal? MeasureValue { get; set; }
        public string MeasureUnit { get; set; } = string.Empty;
        public DateTime? MeasureDate { get; set; }
        public bool? MeasureIsAbnormal { get; set; }
    }
}
