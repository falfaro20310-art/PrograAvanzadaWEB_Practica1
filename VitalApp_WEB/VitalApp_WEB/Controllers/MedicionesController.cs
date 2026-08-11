using Microsoft.AspNetCore.Mvc;
using System.Net;
using VitalApp_WEB.Filters;
using VitalApp_WEB.Models;

namespace VitalApp_WEB.Controllers
{
    [SessionAuthorize]
    public class MedicionesController(
        IHttpClientFactory _http,
        IConfiguration _config) : Controller
    {
        private HttpClient CreateApiClient()
        {
            var client = _http.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + HttpContext.Session.GetString("Token"));
            return client;
        }

        // Consulta los tipos de indicadores de salud disponibles
        private List<IndicatorTypeModel> GetIndicatorTypes()
        {
            using var client = CreateApiClient();

            var urlApi = _config["Valores:UrlApi"] + "User/GetIndicatorTypesAPI";
            var response = client.GetAsync(urlApi).Result;

            if (response.StatusCode != HttpStatusCode.OK)
                return [];

            return response.Content.ReadFromJsonAsync<List<IndicatorTypeModel>>().Result ?? [];
        }

        #region Historial de Mediciones

        [HttpGet]
        public IActionResult Historial(int? IndicatorTypeId, DateTime? DateFrom, DateTime? DateTo)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var urlApi = _config["Valores:UrlApi"] + "User/GetMeasuresAPI?UserId=" + userId;

            if (IndicatorTypeId.HasValue)
                urlApi += "&IndicatorTypeId=" + IndicatorTypeId.Value;
            if (DateFrom.HasValue)
                urlApi += "&DateFrom=" + DateFrom.Value.ToString("yyyy-MM-ddTHH:mm:ss");
            if (DateTo.HasValue)
                urlApi += "&DateTo=" + DateTo.Value.ToString("yyyy-MM-ddTHH:mm:ss");

            using var client = CreateApiClient();

            var response = client.GetAsync(urlApi).Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Salir", "Account");

            var measures = response.StatusCode == HttpStatusCode.OK
                ? response.Content.ReadFromJsonAsync<List<MeasureItemModel>>().Result ?? []
                : [];

            var model = new HistorialMedicionesModel
            {
                IndicatorTypeId = IndicatorTypeId,
                DateFrom = DateFrom,
                DateTo = DateTo,
                Measures = measures,
                IndicatorTypes = GetIndicatorTypes()
            };

            return View(model);
        }

        #endregion

        #region Editar Medicion

        [HttpGet]
        public IActionResult Editar(int measureId)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            using var client = CreateApiClient();

            var urlApi = _config["Valores:UrlApi"] + "User/GetMeasuresAPI?UserId=" + userId + "&MeasureId=" + measureId;
            var response = client.GetAsync(urlApi).Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Salir", "Account");

            var measures = response.Content.ReadFromJsonAsync<List<MeasureItemModel>>().Result;
            var model = measures?.FirstOrDefault();

            if (model == null)
                return RedirectToAction("Historial");

            return View(model);
        }

        [HttpPost]
        public IActionResult Editar(MeasureItemModel model)
        {
            var payload = new
            {
                model.MeasureId,
                model.Value,
                model.SecondaryValue,
                model.MeasureDate,
                model.Notes
            };

            using var client = CreateApiClient();

            var urlApi = _config["Valores:UrlApi"] + "User/UpdateMeasureAPI";
            var response = client.PutAsJsonAsync(urlApi, payload).Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Salir", "Account");

            TempData["MensajeMedicion"] = response.Content.ReadAsStringAsync().Result;
            TempData["ClaseMensajeMedicion"] = response.StatusCode == HttpStatusCode.OK ? "success" : "danger";

            return RedirectToAction("Historial");
        }

        #endregion

        #region Eliminar Medicion

        [HttpPost]
        public IActionResult EliminarMedicion(int measureId)
        {
            using var client = CreateApiClient();

            var urlApi = _config["Valores:UrlApi"] + "User/DeleteMeasureAPI?MeasureId=" + measureId;
            var response = client.DeleteAsync(urlApi).Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Salir", "Account");

            TempData["MensajeMedicion"] = response.Content.ReadAsStringAsync().Result;
            TempData["ClaseMensajeMedicion"] = response.StatusCode == HttpStatusCode.OK ? "success" : "danger";

            return RedirectToAction("Historial");
        }

        #endregion

        #region Registrar Medicion

        [HttpGet]
        public IActionResult Registrar()
        {
            var model = new MeasureModel
            {
                IndicatorTypes = GetIndicatorTypes()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Registrar(MeasureModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var payload = new
            {
                UserId = userId,
                model.IndicatorTypeId,
                model.Value,
                model.SecondaryValue,
                model.MeasureDate,
                model.Notes
            };

            using var client = CreateApiClient();

            var urlApi = _config["Valores:UrlApi"] + "User/RegisterMeasureAPI";
            var response = client.PostAsJsonAsync(urlApi, payload).Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Salir", "Account");

            TempData["MensajeMedicion"] = response.Content.ReadAsStringAsync().Result;
            TempData["ClaseMensajeMedicion"] = response.StatusCode == HttpStatusCode.OK ? "success" : "danger";

            return RedirectToAction("Historial");
        }

        #endregion
    }
}
