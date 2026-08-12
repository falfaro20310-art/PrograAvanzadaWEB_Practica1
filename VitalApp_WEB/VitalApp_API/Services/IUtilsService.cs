namespace VitalApp_API.Services
{
    public interface IUtilsService
    {
        // Genera una contrasena temporal para recuperar el acceso
        string GenerateTemporaryPassword();

        // Envia un correo electronico en formato HTML
        Task SendEmailAsync(string recipient, string subject, string htmlBody);

        // Genera un token JWT para el usuario autenticado
        string GenerateToken(int userId, int roleId, string name);
    }
}
