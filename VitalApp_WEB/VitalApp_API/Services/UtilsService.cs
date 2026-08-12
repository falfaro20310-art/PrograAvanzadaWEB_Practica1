using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System.IdentityModel.Tokens.Jwt;

namespace VitalApp_API.Services
{
    public class UtilsService(IConfiguration _config) : IUtilsService
    {
        // Genera una contrasena temporal para recuperar el acceso
        public string GenerateTemporaryPassword()
        {
            const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var chars = new char[10];

            for (int i = 0; i < 10; i++)
                chars[i] = characters[random.Next(characters.Length)];

            return new string(chars);
        }

        // Envia un correo electronico en formato HTML usando MailKit
        public async Task SendEmailAsync(string recipient, string subject, string htmlBody)
        {
            var host = _config["Smtp:Host"]!;
            var port = int.Parse(_config["Smtp:Port"]!);
            var user = _config["Smtp:User"]!;
            var password = _config["Smtp:Password"]!;

            if (string.IsNullOrEmpty(password))
                return;

            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("VitalApp", user));
            message.To.Add(MailboxAddress.Parse(recipient));
            message.Subject = subject;

            message.Body = new TextPart(TextFormat.Html)
            {
                Text = htmlBody
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(user, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        // Genera un token JWT con el identificador, rol y nombre del usuario
        public string GenerateToken(int userId, int roleId, string name)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = System.Text.Encoding.ASCII.GetBytes(_config["Jwt:SecretKey"]!);
            var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim("userId", userId.ToString()),
                    new System.Security.Claims.Claim("roleId", roleId.ToString()),
                    new System.Security.Claims.Claim("name", name)
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                    new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                    Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
