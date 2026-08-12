namespace VitalApp_WEB.Models
{
    // Detalle de solo lectura de un paciente para el doctor
    public class PatientDetailModel
    {
        public ProfileModel Profile { get; set; } = new();
        public List<DashboardModel> Indicators { get; set; } = [];
    }
}
