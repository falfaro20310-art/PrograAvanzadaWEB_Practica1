namespace VitalApp_WEB.Models
{
    public class IndicatorTypeModel
    {
        public int IndicatorTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal MinNormalValue { get; set; }
        public decimal MaxNormalValue { get; set; }
    }
}
