namespace VitalApp_WEB.Models
{
    // Fila de la tabla de usuarios en el modulo del doctor
    public class UserListItemModel
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string IdCard { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string FullName => $"{Name} {FirstName} {LastName}".Trim();
    }
}
