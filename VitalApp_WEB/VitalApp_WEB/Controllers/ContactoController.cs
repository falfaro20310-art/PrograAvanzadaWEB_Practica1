using Microsoft.AspNetCore.Mvc;
using System.Net;
using VitalApp_WEB.Filters;
using VitalApp_WEB.Models;

namespace VitalApp_WEB.Controllers
{
    [SessionAuthorize]
    public class ContactoController(
        IHttpClientFactory _http,
        IConfiguration _config) : Controller
    {
        // Crea un cliente con el token de la sesion
        private HttpClient CreateApiClient()
        {
            var client = _http.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + HttpContext.Session.GetString("Token"));
            return client;
        }

        #region Chat

        [HttpGet]
        public IActionResult Chat()
        {
            var roleName = HttpContext.Session.GetString("RoleName");
            var isDoctor = roleName == "Doctor";

            using var client = CreateApiClient();

            // Lista de consultas segun el rol
            var urlApi = _config["Valores:UrlApi"] + "Consultation/GetConsultationsAPI";
            var response = client.GetAsync(urlApi).Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Salir", "Account");

            var consultas = response.StatusCode == HttpStatusCode.OK
                ? response.Content.ReadFromJsonAsync<List<ConsultationModel>>().Result ?? []
                : [];

            // El paciente puede adjuntar una medicion como contexto
            if (!isDoctor)
                ViewBag.Measures = GetMeasures();

            ViewBag.IsDoctor = isDoctor;
            ViewBag.Token = HttpContext.Session.GetString("Token");
            ViewBag.UrlHub = _config["Valores:UrlHub"];
            ViewBag.CurrentUserId = HttpContext.Session.GetInt32("UserId");

            return View(consultas);
        }

        // Devuelve el historial de mensajes de una consulta como JSON
        [HttpGet]
        public IActionResult ConsultarMensajes(int consultationId)
        {
            using var client = CreateApiClient();

            var urlApi = _config["Valores:UrlApi"] + "Consultation/GetMessagesAPI?consultationId=" + consultationId;
            var response = client.GetAsync(urlApi).Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return Unauthorized();

            if (response.StatusCode == HttpStatusCode.Forbidden)
                return Forbid();

            var json = response.Content.ReadAsStringAsync().Result;
            return Content(json, "application/json");
        }

        // Mediciones recientes del paciente para el contexto opcional
        private List<MeasureItemModel> GetMeasures()
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            using var client = CreateApiClient();

            var urlApi = _config["Valores:UrlApi"] + "User/GetMeasuresAPI?UserId=" + userId;
            var response = client.GetAsync(urlApi).Result;

            if (response.StatusCode != HttpStatusCode.OK)
                return [];

            return response.Content.ReadFromJsonAsync<List<MeasureItemModel>>().Result ?? [];
        }

        #endregion

        #region Acciones

        // El paciente crea una nueva consulta
        [HttpPost]
        public IActionResult CrearConsulta(string title, string description, int? measureId)
        {
            using var client = CreateApiClient();

            var urlApi = _config["Valores:UrlApi"] + "Consultation/CreateConsultationAPI";
            var response = client.PostAsJsonAsync(urlApi, new
            {
                Title = title,
                Description = description ?? string.Empty,
                MeasureId = measureId
            }).Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Salir", "Account");

            TempData["MensajeChat"] = response.StatusCode == HttpStatusCode.OK
                ? "Tu consulta se creó. Un médico te atenderá pronto."
                : "No se pudo crear la consulta.";
            TempData["ClaseMensajeChat"] = response.StatusCode == HttpStatusCode.OK ? "success" : "danger";

            return RedirectToAction("Chat");
        }

        // Un doctor toma una consulta abierta
        [HttpPost]
        public IActionResult Atender(int consultationId)
        {
            using var client = CreateApiClient();

            var urlApi = _config["Valores:UrlApi"] + "Consultation/TakeConsultationAPI?consultationId=" + consultationId;
            var response = client.PutAsync(urlApi, null).Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Salir", "Account");

            TempData["MensajeChat"] = response.Content.ReadAsStringAsync().Result;
            TempData["ClaseMensajeChat"] = response.StatusCode == HttpStatusCode.OK ? "success" : "danger";

            return RedirectToAction("Chat");
        }

        // Finaliza una consulta
        [HttpPost]
        public IActionResult Finalizar(int consultationId)
        {
            using var client = CreateApiClient();

            var urlApi = _config["Valores:UrlApi"] + "Consultation/CloseConsultationAPI?consultationId=" + consultationId;
            var response = client.PutAsync(urlApi, null).Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Salir", "Account");

            TempData["MensajeChat"] = response.Content.ReadAsStringAsync().Result;
            TempData["ClaseMensajeChat"] = response.StatusCode == HttpStatusCode.OK ? "success" : "danger";

            return RedirectToAction("Chat");
        }

        #endregion
    }
}
