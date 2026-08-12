using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using VitalApp_API.Models;

namespace VitalApp_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IConfiguration _config) : ControllerBase
    {
        // Rol Doctor
        private const int DoctorRoleId = 2;

        // Verifica si quien llama es un doctor segun el claim del token
        private bool IsDoctor()
        {
            var roleId = User.FindFirstValue("roleId");
            return roleId == DoctorRoleId.ToString();
        }

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

        #region Mediciones

        // Consulta los tipos de indicadores de salud disponibles
        [HttpGet("GetIndicatorTypesAPI")]
        public IActionResult GetIndicatorTypesAPI()
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var response = context.Query<IndicatorTypeResponseModel>("GetIndicatorTypesAPI").ToList();

            return Ok(response);
        }

        // Consulta las mediciones de salud del usuario con filtros opcionales
        [HttpGet("GetMeasuresAPI")]
        public IActionResult GetMeasuresAPI(int UserId, int? MeasureId, int? IndicatorTypeId, DateTime? DateFrom, DateTime? DateTo)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", UserId);
            parameters.Add("@MeasureId", MeasureId);
            parameters.Add("@IndicatorTypeId", IndicatorTypeId);
            parameters.Add("@DateFrom", DateFrom);
            parameters.Add("@DateTo", DateTo);

            var response = context.Query<MeasureResponseModel>("usp_UserHealthIndicatorMeasure_Get", parameters).ToList();

            return Ok(response);
        }

        // Actualiza los valores de una medicion de salud existente
        [HttpPut("UpdateMeasureAPI")]
        public IActionResult UpdateMeasureAPI(UpdateMeasureRequestModel model)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@MeasureId", model.MeasureId);
            parameters.Add("@Value", model.Value);
            parameters.Add("@SecondaryValue", model.SecondaryValue);
            parameters.Add("@MeasureDate", model.MeasureDate);
            parameters.Add("@Notes", model.Notes);

            var response = context.Execute("usp_UserHealthIndicatorMeasure_Update", parameters);

            if (response != 0)
                return Ok("La medicion se actualizo correctamente.");

            return BadRequest("La medicion no se pudo actualizar correctamente.");
        }

        // Elimina (logicamente) una medicion de salud del usuario
        [HttpDelete("DeleteMeasureAPI")]
        public IActionResult DeleteMeasureAPI(int MeasureId)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@MeasureId", MeasureId);

            var response = context.Execute("usp_UserHealthIndicatorMeasure_Delete", parameters);

            if (response != 0)
                return Ok("La medicion se elimino correctamente.");

            return BadRequest("La medicion no se pudo eliminar correctamente.");
        }

        // Registra una medicion de indicador de salud para el usuario
        [HttpPost("RegisterMeasureAPI")]
        public IActionResult RegisterMeasureAPI(CreateMeasureRequestModel model)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", model.UserId);
            parameters.Add("@IndicatorTypeId", model.IndicatorTypeId);
            parameters.Add("@Value", model.Value);
            parameters.Add("@SecondaryValue", model.SecondaryValue);
            parameters.Add("@MeasureDate", model.MeasureDate);
            parameters.Add("@Notes", model.Notes);

            var newId = context.QueryFirstOrDefault<int>("usp_UserHealthIndicatorMeasure_Create", parameters);

            if (newId > 0)
                return Ok("La medicion se registro correctamente.");

            return BadRequest("La medicion no se pudo registrar correctamente.");
        }

        #endregion

        #region Gestion de Usuarios (Doctor)

        // Consulta la lista de pacientes (solo para doctores)
        [HttpGet("GetAllPatientsAPI")]
        public IActionResult GetAllPatientsAPI()
        {
            if (!IsDoctor())
                return Forbid();

            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var response = context.Query<UserListItemResponseModel>("spGetAllPatients").ToList();

            return Ok(response);
        }

        // Actualiza el rol de un usuario (solo para doctores)
        [HttpPut("UpdateUserRoleAPI")]
        public IActionResult UpdateUserRoleAPI(ChangeRoleRequestModel model)
        {
            if (!IsDoctor())
                return Forbid();

            // Un doctor no puede cambiar su propio rol
            var currentUserId = User.FindFirstValue("userId");
            if (currentUserId == model.UserId.ToString())
                return BadRequest("No puede cambiar su propio rol.");

            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", model.UserId);
            parameters.Add("@RoleId", model.RoleId);

            var update = context.Execute("spUpdateUserRole", parameters);

            if (update > 0)
                return Ok("El rol se actualizo correctamente.");

            return BadRequest("El rol no se pudo actualizar correctamente.");
        }

        #endregion
    }
}
