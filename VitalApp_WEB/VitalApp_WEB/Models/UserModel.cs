namespace VitalApp_WEB.Models
{
    // Modelo de usuario usado por las vistas y las llamadas al API
    public class UserModel
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string Token { get; set; } = string.Empty;

        // Rol del usuario
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;

        // Datos de perfil
        public string IdCard { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public string Gender { get; set; } = string.Empty;
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
    }
}
