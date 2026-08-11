using Microsoft.AspNetCore.Mvc;
using System.Net;
using VitalApp_WEB.Filters;
using VitalApp_WEB.Models;

namespace VitalApp_WEB.Controllers
{
    [SessionAuthorize]
    public class ObjetivosController(
        IHttpClientFactory _http,
        IConfiguration _config) : Controller
    {
        private HttpClient CreateApiClient()
        {
            var client = _http.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + HttpContext.Session.GetString("Token"));
            return client;
        }

        // Consulta los tipos de indicadores de salud disponibles (para el <select>)
        private List<IndicatorTypeModel> GetIndicatorTypes()
        {
            using var client = CreateApiClient();

            var urlApi = _config["Valores:UrlApi"] + "User/GetIndicatorTypesAPI";
            var response = client.GetAsync(urlApi).Result;

            if (response.StatusCode != HttpStatusCode.OK)
                return [];

            return response.Content.ReadFromJsonAsync<List<IndicatorTypeModel>>().Result ?? [];
        }

        #region Historial de Objetivos

        [HttpGet]
        public IActionResult Historial(string? Status)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var urlApi = _config["Valores:UrlApi"] + "Objective/GetObjectivesAPI?UserId=" + userId;

            if (!string.IsNullOrEmpty(Status))
                urlApi += "&Status=" + Status;

            using var client = CreateApiClient();

            var response = client.GetAsync(urlApi).Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Salir", "Account");

            var objectives = response.StatusCode == HttpStatusCode.OK
                ? response.Content.ReadFromJsonAsync<List<ObjectiveModel>>().Result ?? []
                : [];

            var model = new HistorialObjetivosModel
            {
                Status = Status,
                Objectives = objectives
            };

            return View(model);
        }

        #endregion

        #region Registrar Objetivo

        [HttpGet]
        public IActionResult Registrar()
        {
            var model = new ObjectiveModel
            {
                IndicatorTypes = GetIndicatorTypes()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Registrar(ObjectiveModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var payload = new
            {
                UserId = userId,
                model.IndicatorTypeId,
                model.Title,
                model.Description,
                model.InitialValue,
                model.ObjectiveValue,
                model.StartDate,
                model.LimitDate
            };

            using var client = CreateApiClient();

            var urlApi = _config["Valores:UrlApi"] + "Objective/RegisterObjectiveAPI";
            var response = client.PostAsJsonAsync(urlApi, payload).Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Salir", "Account");

            TempData["MensajeObjetivo"] = response.Content.ReadAsStringAsync().Result;
            TempData["ClaseMensajeObjetivo"] = response.StatusCode == HttpStatusCode.OK ? "success" : "danger";

            return RedirectToAction("Historial");
        }

        #endregion

        #region Editar Objetivo

        [HttpGet]
        public IActionResult Editar(int objectiveId)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            using var client = CreateApiClient();

            var urlApi = _config["Valores:UrlApi"] + "Objective/GetObjectivesAPI?UserId=" + userId + "&ObjectiveId=" + objectiveId;
            var response = client.GetAsync(urlApi).Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Salir", "Account");

            var objectives = response.Content.ReadFromJsonAsync<List<ObjectiveModel>>().Result;
            var model = objectives?.FirstOrDefault();

            if (model == null)
                return RedirectToAction("Historial");

            return View(model);
        }

        [HttpPost]
        public IActionResult Editar(ObjectiveModel model)
        {
            var payload = new
            {
                model.ObjectiveId,
                model.Title,
                model.Description,
                model.ObjectiveValue,
                model.LimitDate,
                model.Status
            };

            using var client = CreateApiClient();

            var urlApi = _config["Valores:UrlApi"] + "Objective/UpdateObjectiveAPI";
            var response = client.PutAsJsonAsync(urlApi, payload).Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Salir", "Account");

            TempData["MensajeObjetivo"] = response.Content.ReadAsStringAsync().Result;
            TempData["ClaseMensajeObjetivo"] = response.StatusCode == HttpStatusCode.OK ? "success" : "danger";

            return RedirectToAction("Historial");
        }

        #endregion

        #region Eliminar Objetivo

        [HttpPost]
        public IActionResult EliminarObjetivo(int objectiveId)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            using var client = CreateApiClient();

            var urlApi = _config["Valores:UrlApi"] + "Objective/DeleteObjectiveAPI?ObjectiveId=" + objectiveId + "&UserId=" + userId;
            var response = client.DeleteAsync(urlApi).Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Salir", "Account");

            TempData["MensajeObjetivo"] = response.Content.ReadAsStringAsync().Result;
            TempData["ClaseMensajeObjetivo"] = response.StatusCode == HttpStatusCode.OK ? "success" : "danger";

            return RedirectToAction("Historial");
        }

        #endregion

        #region Registrar Avance

        [HttpPost]
        public IActionResult RegistrarAvance(int objectiveId, decimal currentValue)
        {
            var payload = new
            {
                ObjectiveId = objectiveId,
                Date = DateTime.Now,
                CurrentValue = currentValue
            };

            using var client = CreateApiClient();

            var urlApi = _config["Valores:UrlApi"] + "Objective/RegisterProgressAPI";
            var response = client.PostAsJsonAsync(urlApi, payload).Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Salir", "Account");

            TempData["MensajeObjetivo"] = response.Content.ReadAsStringAsync().Result;
            TempData["ClaseMensajeObjetivo"] = response.StatusCode == HttpStatusCode.OK ? "success" : "danger";

            return RedirectToAction("Historial");
        }

        #endregion
    }
}
