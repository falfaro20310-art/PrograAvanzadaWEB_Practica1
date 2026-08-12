using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using VitalApp_API.Models;
using VitalApp_API.Services;

namespace VitalApp_API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController(IConfiguration _config, IUtilsService _utils) : ControllerBase
    {
        // Registra un nuevo usuario con su perfil
        [HttpPost("RegisterAPI")]
        public IActionResult RegisterAPI(RegisterUserRequestModel model)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@Email", model.Email);
            parameters.Add("@Password", model.Password);
            parameters.Add("@IdCard", model.IdCard);
            parameters.Add("@Name", model.Name);
            parameters.Add("@FirstName", model.FirstName);
            parameters.Add("@LastName", model.LastName);

            var response = context.Execute("spRegisterUser", parameters);

            if (response > 0)
                return Ok(response);

            return BadRequest("El correo o la identificacion ya se encuentran registrados.");
        }

        // Valida las credenciales y genera el token de acceso
        [HttpPost("LoginAPI")]
        public IActionResult LoginAPI(LoginRequestModel model)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@Email", model.Email);

            var response = context.QueryFirstOrDefault<UserResponseModel>("spLoginUser", parameters);

            if (response != null && BCrypt.Net.BCrypt.Verify(model.Password, response.Password))
            {
                response.Token = _utils.GenerateToken(response.UserId, response.RoleId, response.Name);

                // No se devuelve el hash de la contrasena
                response.Password = string.Empty;

                return Ok(response);
            }

            // Mensaje generico para no revelar si fallo el correo o la contrasena
            return NotFound("Credenciales inválidas");
        }

        // Genera una contrasena temporal y la envia al correo del usuario
        [HttpPost("RecoverPasswordAPI")]
        public async Task<IActionResult> RecoverPasswordAPI(RecoverPasswordRequestModel model)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@Email", model.Email);

            var response = context.QueryFirstOrDefault<UserResponseModel>("spValidateEmail", parameters);

            if (response == null)
                return NotFound("El correo no se encuentra registrado.");

            // Se genera la contrasena temporal y se guarda cifrada
            var temporary = _utils.GenerateTemporaryPassword();
            var temporaryHash = BCrypt.Net.BCrypt.HashPassword(temporary);

            parameters = new DynamicParameters();
            parameters.Add("@UserId", response.UserId);
            parameters.Add("@Password", temporaryHash);

            var update = context.Execute("spUpdatePassword", parameters);

            if (update > 0)
            {
                // Se envia la contrasena temporal por correo
                string path = Path.Combine(AppContext.BaseDirectory, "Templates", "PasswordRecovery.html");
                string template = System.IO.File.ReadAllText(path);

                template = template.Replace("{{NAME}}", response.Name);
                template = template.Replace("{{TEMPORARY}}", temporary);

                await _utils.SendEmailAsync(response.Email, "Recuperación de contraseña", template);

                return Ok(response);
            }

            return BadRequest("No se pudo recuperar su acceso, por favor intente nuevamente.");
        }
    }
}
