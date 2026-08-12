using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using VitalApp_API.Models;

namespace VitalApp_API.Hubs
{
    [Authorize]
    public class ChatHub(IConfiguration _config) : Hub
    {
        // Identificador del usuario autenticado en la conexion
        private int CurrentUserId =>
            int.TryParse(Context.User?.FindFirstValue("userId"), out var id) ? id : 0;

        // Nombre del usuario autenticado en la conexion
        private string CurrentName =>
            Context.User?.FindFirstValue("name") ?? string.Empty;

        // Une la conexion a la sala de una consulta si tiene acceso
        public async Task JoinRoom(int consultationId)
        {
            if (!HasAccess(consultationId))
                throw new HubException("Acceso denegado a esta conversación.");

            await Groups.AddToGroupAsync(Context.ConnectionId, RoomName(consultationId));
        }

        // Registra y difunde un mensaje a la sala de la consulta
        public async Task SendMessage(int consultationId, string content)
        {
            if (!HasAccess(consultationId))
                throw new HubException("Acceso denegado a esta conversación.");

            if (string.IsNullOrWhiteSpace(content))
                return;

            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@ConsultationId", consultationId);
            parameters.Add("@SenderUserId", CurrentUserId);
            parameters.Add("@Content", content);

            var messageId = context.QuerySingle<int>("spRegisterMessage", parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            var message = new MessageResponseModel
            {
                MessageId = messageId,
                ConsultationId = consultationId,
                SenderUserId = CurrentUserId,
                SenderName = CurrentName,
                Content = content,
                SentAt = DateTime.UtcNow
            };

            await Clients.Group(RoomName(consultationId)).SendAsync("ReceiveMessage", message);
        }

        // Valida que el usuario sea el paciente o el doctor de la consulta
        private bool HasAccess(int consultationId)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@ConsultationId", consultationId);
            parameters.Add("@UserId", CurrentUserId);

            return context.QuerySingle<int>("spValidateConsultationAccess", parameters,
                commandType: System.Data.CommandType.StoredProcedure) > 0;
        }

        private static string RoomName(int consultationId) => $"consultation-{consultationId}";
    }
}
