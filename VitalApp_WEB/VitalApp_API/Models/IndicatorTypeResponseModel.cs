namespace VitalApp_API.Models
{
    public class IndicatorTypeResponseModel
    {
        public int IndicatorTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal MinNormalValue { get; set; }
        public decimal MaxNormalValue { get; set; }
    }
}
