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
    public class ObjectiveController(IConfiguration _config) : ControllerBase
    {
        #region Objetivos

        // Consulta los objetivos de salud del usuario, con filtros opcionales
        [HttpGet("GetObjectivesAPI")]
        public IActionResult GetObjectivesAPI(int UserId, int? ObjectiveId, string? Status)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", UserId);
            parameters.Add("@ObjectiveId", ObjectiveId);
            parameters.Add("@Status", Status);

            var response = context.Query<ObjectiveResponseModel>("usp_UserObjective_Get", parameters).ToList();

            return Ok(response);
        }

        // Registra un nuevo objetivo de salud
        [HttpPost("RegisterObjectiveAPI")]
        public IActionResult RegisterObjectiveAPI(ObjectiveRequestModel model)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", model.UserId);
            parameters.Add("@IndicatorTypeId", model.IndicatorTypeId);
            parameters.Add("@Title", model.Title);
            parameters.Add("@Description", model.Description);
            parameters.Add("@InitialValue", model.InitialValue);
            parameters.Add("@ObjectiveValue", model.ObjectiveValue);
            parameters.Add("@StartDate", model.StartDate);
            parameters.Add("@LimitDate", model.LimitDate);

            var newId = context.QueryFirstOrDefault<int>("usp_UserObjective_Create", parameters);

            if (newId > 0)
                return Ok("El objetivo se registro correctamente.");

            return BadRequest("El objetivo no se pudo registrar correctamente.");
        }

        // Actualiza un objetivo de salud existente
        [HttpPut("UpdateObjectiveAPI")]
        public IActionResult UpdateObjectiveAPI(UpdateObjectiveRequestModel model)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@ObjectiveId", model.ObjectiveId);
            parameters.Add("@Title", model.Title);
            parameters.Add("@Description", model.Description);
            parameters.Add("@ObjectiveValue", model.ObjectiveValue);
            parameters.Add("@LimitDate", model.LimitDate);
            parameters.Add("@Status", model.Status);

            var response = context.Execute("usp_UserObjective_Update", parameters);

            if (response > 0)
                return Ok("El objetivo se actualizo correctamente.");

            return BadRequest("El objetivo no se pudo actualizar correctamente.");
        }

        // Elimina (logicamente) un objetivo de salud
        [HttpDelete("DeleteObjectiveAPI")]
        public IActionResult DeleteObjectiveAPI(int ObjectiveId, int UserId)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@ObjectiveId", ObjectiveId);
            parameters.Add("@UserId", UserId);

            var response = context.Execute("usp_UserObjective_Delete", parameters);

            if (response > 0)
                return Ok("El objetivo se elimino correctamente.");

            return BadRequest("El objetivo no se pudo eliminar correctamente.");
        }

        #endregion

        #region Avance del objetivo

        // Registra un nuevo avance para un objetivo
        [HttpPost("RegisterProgressAPI")]
        public IActionResult RegisterProgressAPI(ObjectiveProgressRequestModel model)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@ObjectiveId", model.ObjectiveId);
            parameters.Add("@Date", model.Date);
            parameters.Add("@CurrentValue", model.CurrentValue);

            var response = context.QueryFirstOrDefault<int?>("usp_UserObjective_RegisterProgress", parameters);

            if (response != null && response > 0)
                return Ok("El avance se registro correctamente.");

            return BadRequest("El avance no se pudo registrar correctamente.");
        }

        // Consulta el historial de avance de un objetivo
        [HttpGet("GetProgressAPI")]
        public IActionResult GetProgressAPI(int ObjectiveId)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@ObjectiveId", ObjectiveId);

            var response = context.Query<ObjectiveProgressResponseModel>("usp_UserObjective_GetProgress", parameters).ToList();

            return Ok(response);
        }

        #endregion
    }
}
