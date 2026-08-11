using Microsoft.AspNetCore.Mvc;
using System.Net;
using VitalApp_WEB.Filters;
using VitalApp_WEB.Models;

namespace VitalApp_WEB.Controllers
{
    [SessionAuthorize]
    public class HistorialController(
        IHttpClientFactory _http,
        IConfiguration _config) : Controller
    {
        private HttpClient CreateApiClient()
        {
            var client = _http.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + HttpContext.Session.GetString("Token"));
            return client;
        }

        #region Linea de tiempo unificada

        [HttpGet]
        public IActionResult Index(string? EventType, DateTime? DateFrom, DateTime? DateTo)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            using var client = CreateApiClient();

            var measures = GetMediciones(client, userId, DateFrom, DateTo);
            if (measures == null)
                return RedirectToAction("Salir", "Account");

            var progressEvents = GetAvancesObjetivos(client, userId, DateFrom, DateTo);
            if (progressEvents == null)
                return RedirectToAction("Salir", "Account");

            var conditions = GetCondicionesMedicas(client, userId, DateFrom, DateTo);
            if (conditions == null)
                return RedirectToAction("Salir", "Account");

            var events = new List<TimelineEventModel>();
            events.AddRange(measures);
            events.AddRange(progressEvents);
            events.AddRange(conditions);

            var model = new HistorialModel
            {
                EventType = EventType,
                DateFrom = DateFrom,
                DateTo = DateTo,
                TotalMediciones = measures.Count,
                TotalAvances = progressEvents.Count,
                TotalCondiciones = conditions.Count,
                Events = events
                    .Where(e => string.IsNullOrEmpty(EventType) || e.EventType == EventType)
                    .OrderByDescending(e => e.EventDate)
                    .ToList()
            };

            return View(model);
        }

        // Consulta las mediciones de salud del usuario y las convierte en eventos de la linea de tiempo
        private List<TimelineEventModel>? GetMediciones(HttpClient client, int userId, DateTime? dateFrom, DateTime? dateTo)
        {
            var urlApi = _config["Valores:UrlApi"] + "User/GetMeasuresAPI?UserId=" + userId;

            if (dateFrom.HasValue)
                urlApi += "&DateFrom=" + dateFrom.Value.ToString("yyyy-MM-ddTHH:mm:ss");
            if (dateTo.HasValue)
                urlApi += "&DateTo=" + dateTo.Value.ToString("yyyy-MM-ddTHH:mm:ss");

            var response = client.GetAsync(urlApi).Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return null;

            var measures = response.StatusCode == HttpStatusCode.OK
                ? response.Content.ReadFromJsonAsync<List<MeasureItemModel>>().Result ?? []
                : [];

            return measures.Select(m => new TimelineEventModel
            {
                EventDate = m.MeasureDate,
                EventType = "Medicion",
                Title = m.IndicatorTypeName,
                Detail = m.Value + " " + m.Unit + (m.SecondaryValue > 0 ? " / " + m.SecondaryValue + " " + m.Unit : ""),
                IsAbnormal = m.IsAbnormal
            }).ToList();
        }

        // Consulta los objetivos del usuario y, para cada uno, su historial de avances
        private List<TimelineEventModel>? GetAvancesObjetivos(HttpClient client, int userId, DateTime? dateFrom, DateTime? dateTo)
        {
            var urlObjetivos = _config["Valores:UrlApi"] + "Objective/GetObjectivesAPI?UserId=" + userId;
            var responseObjetivos = client.GetAsync(urlObjetivos).Result;

            if (responseObjetivos.StatusCode == HttpStatusCode.Unauthorized)
                return null;

            var objetivos = responseObjetivos.StatusCode == HttpStatusCode.OK
                ? responseObjetivos.Content.ReadFromJsonAsync<List<ObjectiveModel>>().Result ?? []
                : [];

            var events = new List<TimelineEventModel>();

            foreach (var objetivo in objetivos)
            {
                var urlProgreso = _config["Valores:UrlApi"] + "Objective/GetProgressAPI?ObjectiveId=" + objetivo.ObjectiveId;
                var responseProgreso = client.GetAsync(urlProgreso).Result;

                if (responseProgreso.StatusCode == HttpStatusCode.Unauthorized)
                    return null;

                var avances = responseProgreso.StatusCode == HttpStatusCode.OK
                    ? responseProgreso.Content.ReadFromJsonAsync<List<ObjectiveProgressModel>>().Result ?? []
                    : [];

                events.AddRange(avances
                    .Where(a => (!dateFrom.HasValue || a.Date >= dateFrom.Value) && (!dateTo.HasValue || a.Date <= dateTo.Value))
                    .Select(a => new TimelineEventModel
                    {
                        EventDate = a.Date,
                        EventType = "Avance",
                        Title = objetivo.Title,
                        Detail = "Avance registrado: " + a.CurrentValue + " " + objetivo.Unit + " (" + a.ComplianceRate + "% cumplido)"
                    }));
            }

            return events;
        }

        // Consulta las condiciones medicas del usuario y las convierte en eventos de la linea de tiempo
        private List<TimelineEventModel>? GetCondicionesMedicas(HttpClient client, int userId, DateTime? dateFrom, DateTime? dateTo)
        {
            var urlApi = _config["Valores:UrlApi"] + "User/GetMedicalConditionsAPI?UserId=" + userId;
            var response = client.GetAsync(urlApi).Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return null;

            var condiciones = response.StatusCode == HttpStatusCode.OK
                ? response.Content.ReadFromJsonAsync<List<MedicalConditionModel>>().Result ?? []
                : [];

            return condiciones
                .Where(c => (!dateFrom.HasValue || c.DiagnosticDate >= dateFrom.Value) && (!dateTo.HasValue || c.DiagnosticDate <= dateTo.Value))
                .Select(c => new TimelineEventModel
                {
                    EventDate = c.DiagnosticDate,
                    EventType = "Condicion",
                    Title = c.Name,
                    Detail = c.Description
                }).ToList();
        }

        #endregion
    }
}
