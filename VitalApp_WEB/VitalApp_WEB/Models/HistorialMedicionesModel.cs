namespace VitalApp_WEB.Models
{
    // Modelo de la pantalla de historial de mediciones (filtros + resultados)
    public class HistorialMedicionesModel
    {
        public int? IndicatorTypeId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public List<MeasureItemModel> Measures { get; set; } = [];
        public List<IndicatorTypeModel> IndicatorTypes { get; set; } = [];
    }
}
