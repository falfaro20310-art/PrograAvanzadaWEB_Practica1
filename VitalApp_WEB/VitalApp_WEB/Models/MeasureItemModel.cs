namespace VitalApp_WEB.Models
{
    // Representa una medicion de salud dentro del historial
    public class MeasureItemModel
    {
        public int MeasureId { get; set; }
        public int IndicatorTypeId { get; set; }
        public string IndicatorTypeName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal SecondaryValue { get; set; }
        public DateTime MeasureDate { get; set; }
        public string? Notes { get; set; }
        public bool IsAbnormal { get; set; }
    }
}
