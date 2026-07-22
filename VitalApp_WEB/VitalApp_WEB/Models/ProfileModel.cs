namespace VitalApp_WEB.Models
{
    // Datos del perfil que se muestran y editan en la pantalla de perfil
    public class ProfileModel
    {
        public int UserId { get; set; }
        public int ProfileId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string IdCard { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public string Gender { get; set; } = string.Empty;
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }

        // Cambio de contrasena
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;

        public List<MedicalConditionModel> MedicalConditions { get; set; } = [];

        // Edad calculada a partir de la fecha de nacimiento
        public int? Age
        {
            get
            {
                if (BirthDate == null) return null;

                var age = DateTime.Today.Year - BirthDate.Value.Year;
                if (BirthDate.Value.Date > DateTime.Today.AddYears(-age)) age--;

                return age;
            }
        }
    }
}
