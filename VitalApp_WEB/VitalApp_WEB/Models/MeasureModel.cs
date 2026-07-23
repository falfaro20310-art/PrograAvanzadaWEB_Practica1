namespace VitalApp_WEB.Models
{
    // Datos del formulario para registrar una medicion de salud
    public class MeasureModel
    {
        public int IndicatorTypeId { get; set; }
        public decimal Value { get; set; }
        public decimal SecondaryValue { get; set; }
        public DateTime MeasureDate { get; set; } = DateTime.Today;
        public string? Notes { get; set; }

        public List<IndicatorTypeModel> IndicatorTypes { get; set; } = [];
    }
}
