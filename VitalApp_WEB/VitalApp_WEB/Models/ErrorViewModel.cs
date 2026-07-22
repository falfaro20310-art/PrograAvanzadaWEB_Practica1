namespace VitalApp_WEB.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        // Detalle tecnico: solo se muestra en Development
        public bool ShowDetails { get; set; }

        public string? Message { get; set; }

        public string? Path { get; set; }

        public string? StackTrace { get; set; }
    }
}
