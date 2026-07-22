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
                response.Token = _utils.GenerateToken(response.UserId);

                // No se devuelve el hash de la contrasena
                response.Password = string.Empty;

                return Ok(response);
            }

            // Mensaje generico para no revelar si fallo el correo o la contrasena
            return NotFound("Credenciales inválidas");
        }
    }
}
