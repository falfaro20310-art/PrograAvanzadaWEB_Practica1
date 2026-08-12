using Microsoft.AspNetCore.Mvc;
using System.Net;
using VitalApp_WEB.Filters;
using VitalApp_WEB.Models;

namespace VitalApp_WEB.Controllers
{
    [DoctorAuthorize]
    public class UsuariosController(
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

        #region Lista de Pacientes

        [HttpGet]
        public IActionResult Index()
        {
            using var client = CreateApiClient();

            var urlApi = _config["Valores:UrlApi"] + "User/GetAllPatientsAPI";
            var response = client.GetAsync(urlApi).Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Salir", "Account");

            var pacientes = response.StatusCode == HttpStatusCode.OK
                ? response.Content.ReadFromJsonAsync<List<UserListItemModel>>().Result ?? []
                : [];

            return View(pacientes);
        }

        #endregion

        #region Detalle de Paciente (solo lectura)

        [HttpGet]
        public IActionResult Detalle(int id)
        {
            using var client = CreateApiClient();
            var baseUrl = _config["Valores:UrlApi"];

            // Perfil del paciente
            var profileResponse = client.GetAsync(baseUrl + "User/GetProfileAPI?UserId=" + id).Result;

            if (profileResponse.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Salir", "Account");

            if (profileResponse.StatusCode != HttpStatusCode.OK)
                return RedirectToAction("Index");

            var model = new PatientDetailModel
            {
                Profile = profileResponse.Content.ReadFromJsonAsync<ProfileModel>().Result!
            };

            // Condiciones medicas del paciente
            var conditionsResponse = client.GetAsync(baseUrl + "User/GetMedicalConditionsAPI?UserId=" + id).Result;
            if (conditionsResponse.StatusCode == HttpStatusCode.OK)
                model.Profile.MedicalConditions = conditionsResponse.Content.ReadFromJsonAsync<List<MedicalConditionModel>>().Result ?? [];

            // Indicadores de salud del paciente
            var dashboardResponse = client.GetAsync(baseUrl + "Dashboard/GetDashboard/" + id).Result;
            if (dashboardResponse.StatusCode == HttpStatusCode.OK)
                model.Indicators = dashboardResponse.Content.ReadFromJsonAsync<List<DashboardModel>>().Result ?? [];

            return View(model);
        }

        #endregion

        #region Cambiar Rol

        [HttpPost]
        public IActionResult CambiarRol(int userId, int roleId)
        {
            using var client = CreateApiClient();

            var urlApi = _config["Valores:UrlApi"] + "User/UpdateUserRoleAPI";
            var response = client.PutAsJsonAsync(urlApi, new { UserId = userId, RoleId = roleId }).Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Salir", "Account");

            TempData["MensajeUsuarios"] = response.Content.ReadAsStringAsync().Result;
            TempData["ClaseMensajeUsuarios"] = response.StatusCode == HttpStatusCode.OK ? "success" : "danger";

            return RedirectToAction("Index");
        }

        #endregion
    }
}
