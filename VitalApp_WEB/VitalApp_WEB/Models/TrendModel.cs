namespace VitalApp_WEB.Models
{
    public class TrendModel
    {
        public int IndicatorTypeId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public int MeasureId { get; set; }

        public decimal Value { get; set; }

        public decimal SecondaryValue { get; set; }

        public DateTime MeasureDate { get; set; }

        public bool IsAbnormal { get; set; }
    }
}