using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using VitalApp_API.Hubs;
using VitalApp_API.Models;

namespace VitalApp_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultationController(
        IConfiguration _config,
        IHubContext<ChatHub> _hub) : ControllerBase
    {
        private const int PatientRoleId = 1;
        private const int DoctorRoleId = 2;

        // Identificador del usuario autenticado segun el token
        private int CurrentUserId =>
            int.TryParse(User.FindFirstValue("userId"), out var id) ? id : 0;

        // Rol del usuario autenticado segun el token
        private int CurrentRoleId =>
            int.TryParse(User.FindFirstValue("roleId"), out var id) ? id : 0;

        // Nombre del usuario autenticado segun el token
        private string CurrentName => User.FindFirstValue("name") ?? string.Empty;

        private static string RoomName(int consultationId) => $"consultation-{consultationId}";

        // Valida que el usuario sea participante de la consulta
        private bool HasAccess(int consultationId)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@ConsultationId", consultationId);
            parameters.Add("@UserId", CurrentUserId);

            return context.QuerySingle<int>("spValidateConsultationAccess", parameters) > 0;
        }

        // Lista las consultas segun el rol (paciente o doctor)
        [HttpGet("GetConsultationsAPI")]
        public IActionResult GetConsultationsAPI()
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", CurrentUserId);
            parameters.Add("@RoleId", CurrentRoleId);

            var response = context.Query<ConsultationResponseModel>("spGetConsultations", parameters).ToList();

            return Ok(response);
        }

        // Consulta el historial de mensajes de una consulta
        [HttpGet("GetMessagesAPI")]
        public IActionResult GetMessagesAPI(int consultationId)
        {
            if (!HasAccess(consultationId))
                return Forbid();

            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@ConsultationId", consultationId);

            var response = context.Query<MessageResponseModel>("spGetMessages", parameters).ToList();

            return Ok(response);
        }

        // Crea una consulta (solo el paciente)
        [HttpPost("CreateConsultationAPI")]
        public IActionResult CreateConsultationAPI(CreateConsultationRequestModel model)
        {
            if (CurrentRoleId != PatientRoleId)
                return Forbid();

            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@PatientUserId", CurrentUserId);
            parameters.Add("@Title", model.Title);
            parameters.Add("@Description", model.Description);
            parameters.Add("@MeasureId", model.MeasureId);

            var consultationId = context.QuerySingle<int>("spCreateConsultation", parameters);

            return Ok(consultationId);
        }

        // Un doctor toma una consulta abierta
        [HttpPut("TakeConsultationAPI")]
        public async Task<IActionResult> TakeConsultationAPI(int consultationId)
        {
            if (CurrentRoleId != DoctorRoleId)
                return Forbid();

            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@ConsultationId", consultationId);
            parameters.Add("@DoctorUserId", CurrentUserId);

            var update = context.Execute("spTakeConsultation", parameters);

            if (update > 0)
            {
                // Avisa en vivo a la sala que la consulta fue asignada
                await _hub.Clients.Group(RoomName(consultationId)).SendAsync("ConsultationUpdated", new
                {
                    consultationId,
                    statusId = 2,
                    statusName = "InProgress",
                    doctorName = CurrentName
                });

                return Ok("Consulta asignada correctamente.");
            }

            return BadRequest("La consulta ya fue tomada o no está disponible.");
        }

        // El paciente o el doctor finaliza una consulta
        [HttpPut("CloseConsultationAPI")]
        public async Task<IActionResult> CloseConsultationAPI(int consultationId)
        {
            if (!HasAccess(consultationId))
                return Forbid();

            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@ConsultationId", consultationId);
            parameters.Add("@UserId", CurrentUserId);

            var update = context.Execute("spCloseConsultation", parameters);

            if (update > 0)
            {
                // Avisa en vivo a la sala que la consulta se finalizo
                await _hub.Clients.Group(RoomName(consultationId)).SendAsync("ConsultationUpdated", new
                {
                    consultationId,
                    statusId = 3,
                    statusName = "Closed",
                    doctorName = (string?)null
                });

                return Ok("Consulta finalizada.");
            }

            return BadRequest("La consulta ya estaba finalizada.");
        }
    }
}
