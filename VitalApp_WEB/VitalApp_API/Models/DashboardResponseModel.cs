namespace VitalApp_API.Models
{
    public class DashboardResponseModel
    {
        public int IndicatorTypeId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public decimal MinNormalValue { get; set; }

        public decimal MaxNormalValue { get; set; }

        public decimal? LastValue { get; set; }

        public decimal? SecondaryValue { get; set; }

        public DateTime? LastMeasureDate { get; set; }

        public bool? IsAbnormal { get; set; }

        public decimal? WeeklyAverage { get; set; }

        public decimal? WeeklyMinimum { get; set; }

        public decimal? WeeklyMaximum { get; set; }

        public decimal? MonthlyAverage { get; set; }

        public decimal? MonthlyMinimum { get; set; }

        public decimal? MonthlyMaximum { get; set; }
    }
}