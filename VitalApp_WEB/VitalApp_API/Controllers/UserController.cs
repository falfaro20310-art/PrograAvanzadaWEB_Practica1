using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using VitalApp_API.Models;

namespace VitalApp_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IConfiguration _config) : ControllerBase
    {
        #region Perfil

        // Consulta los datos personales del usuario
        [HttpGet("GetProfileAPI")]
        public IActionResult GetProfileAPI(int UserId)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", UserId);

            var response = context.QueryFirstOrDefault<ProfileResponseModel>("spGetProfile", parameters);

            if (response != null)
                return Ok(response);

            return NotFound("El perfil no se pudo encontrar.");
        }

        // Actualiza los datos personales del usuario
        [HttpPut("UpdateProfileAPI")]
        public IActionResult UpdateProfileAPI(UpdateProfileRequestModel model)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", model.UserId);
            parameters.Add("@Name", model.Name);
            parameters.Add("@FirstName", model.FirstName);
            parameters.Add("@LastName", model.LastName);
            parameters.Add("@BirthDate", model.BirthDate);
            parameters.Add("@Gender", model.Gender);
            parameters.Add("@Height", model.Height);
            parameters.Add("@Weight", model.Weight);

            var update = context.Execute("spUpdateProfile", parameters);

            if (update > 0)
                return Ok("Sus datos se actualizaron correctamente.");

            return BadRequest("Su informacion no se pudo actualizar correctamente.");
        }

        // Actualiza la contrasena del usuario
        [HttpPut("ChangePasswordAPI")]
        public IActionResult ChangePasswordAPI(ChangePasswordRequestModel model)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", model.UserId);
            parameters.Add("@Password", model.Password);

            var update = context.Execute("spUpdatePassword", parameters);

            if (update > 0)
                return Ok("Su contrasena se actualizo correctamente.");

            return BadRequest("La contrasena no se pudo actualizar correctamente.");
        }

        #endregion

        #region Condiciones Medicas

        // Consulta las condiciones medicas del usuario
        [HttpGet("GetMedicalConditionsAPI")]
        public IActionResult GetMedicalConditionsAPI(int UserId)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", UserId);

            var response = context.Query<MedicalConditionResponseModel>("spGetMedicalConditions", parameters).ToList();

            return Ok(response);
        }

        // Registra una condicion medica del usuario
        [HttpPost("RegisterMedicalConditionAPI")]
        public IActionResult RegisterMedicalConditionAPI(MedicalConditionRequestModel model)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", model.UserId);
            parameters.Add("@Name", model.Name);
            parameters.Add("@Description", model.Description);
            parameters.Add("@DiagnosticDate", model.DiagnosticDate);

            var response = context.Execute("spRegisterMedicalCondition", parameters);

            if (response > 0)
                return Ok("La condicion medica se registro correctamente.");

            return BadRequest("La condicion medica no se pudo registrar correctamente.");
        }

        // Elimina (logicamente) una condicion medica del usuario
        [HttpDelete("DeleteMedicalConditionAPI")]
        public IActionResult DeleteMedicalConditionAPI(int MedicalConditionId, int UserId)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@MedicalConditionId", MedicalConditionId);
            parameters.Add("@UserId", UserId);

            var response = context.Execute("spDeleteMedicalCondition", parameters);

            if (response > 0)
                return Ok("La condicion medica se elimino correctamente.");

            return BadRequest("La condicion medica no se pudo eliminar correctamente.");
        }

        #endregion
    }
}
