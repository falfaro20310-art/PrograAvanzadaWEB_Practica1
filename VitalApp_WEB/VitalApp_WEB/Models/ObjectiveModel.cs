namespace VitalApp_WEB.Models
{
    // Objetivo de salud, usado tanto para listar como para el formulario
    public class ObjectiveModel
    {
        public int ObjectiveId { get; set; }
        public int UserId { get; set; }
        public int IndicatorTypeId { get; set; }
        public string IndicatorTypeName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? InitialValue { get; set; }
        public decimal? ObjectiveValue { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime? LimitDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? LastValue { get; set; }
        public decimal? ComplianceRate { get; set; }

        public List<IndicatorTypeModel> IndicatorTypes { get; set; } = [];
    }

    // Filtros e items para la pantalla de historial de objetivos
    public class HistorialObjetivosModel
    {
        public string? Status { get; set; }
        public List<ObjectiveModel> Objectives { get; set; } = [];
    }

    // Registro de avance de un objetivo
    public class ObjectiveProgressModel
    {
        public int ObjectiveProgressId { get; set; }
        public int ObjectiveId { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public decimal CurrentValue { get; set; }
        public decimal ComplianceRate { get; set; }
    }
}
