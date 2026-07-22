using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System.IdentityModel.Tokens.Jwt;

namespace VitalApp_API.Services
{
    public class UtilsService(IConfiguration _config) : IUtilsService
    {
        // Genera un codigo numerico de 6 digitos para recuperar el acceso
        public string GenerateRecoveryCode()
        {
            var random = new Random();
            return random.Next(100000, 1000000).ToString();
        }

        // Envia un correo electronico en formato HTML usando MailKit
        public async Task SendEmailAsync(string recipient, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            var email = _config["Emails:Email"]!;
            var appPassword = _config["Emails:AppPassword"]!;

            if (string.IsNullOrEmpty(appPassword))
                return;

            message.From.Add(new MailboxAddress(string.Empty, email));
            message.To.Add(MailboxAddress.Parse(recipient));
            message.Subject = subject;

            message.Body = new TextPart(TextFormat.Html)
            {
                Text = htmlBody
            };

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(email, appPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        // Genera un token JWT con el identificador del usuario
        public string GenerateToken(int userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = System.Text.Encoding.ASCII.GetBytes(_config["Jwt:SecretKey"]!);
            var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim("userId", userId.ToString())
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
