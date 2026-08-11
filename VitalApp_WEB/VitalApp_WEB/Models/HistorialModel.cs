namespace VitalApp_WEB.Models
{
    // Un evento dentro de la linea de tiempo unificada (medicion, avance de objetivo o condicion medica)
    public class TimelineEventModel
    {
        public DateTime EventDate { get; set; }
        public string EventType { get; set; } = string.Empty; // Medicion | Avance | Condicion
        public string Title { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public bool IsAbnormal { get; set; }
    }

    // Modelo de la pantalla de historial unificado (filtros + linea de tiempo)
    public class HistorialModel
    {
        public string? EventType { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public List<TimelineEventModel> Events { get; set; } = [];

        public int TotalMediciones { get; set; }
        public int TotalAvances { get; set; }
        public int TotalCondiciones { get; set; }
    }
}
