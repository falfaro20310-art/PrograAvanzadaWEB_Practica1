using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.RegularExpressions;
using VitalApp_WEB.Filters;
using VitalApp_WEB.Models;

namespace VitalApp_WEB.Controllers
{
    [SessionAuthorize]
    public class PerfilController(
        IHttpClientFactory _http,
        IConfiguration _config) : Controller
    {
        // Expresion para validar la seguridad minima de la contrasena
        private const string PasswordPattern =
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$";

        // Crea un cliente con el token de la sesion
        private HttpClient CreateApiClient()
        {
            var client = _http.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + HttpContext.Session.GetString("Token"));
            return client;
        }

        #region Consultar Perfil

        [HttpGet]
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            using var client = CreateApiClient();

            var urlApi = _config["Valores:UrlApi"] + "User/GetProfileAPI?UserId=" + userId;
            var response = client.GetAsync(urlApi).Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Salir", "Account");

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("Ocurrio un error al intentar consultar su perfil.");

            var model = response.Content.ReadFromJsonAsync<ProfileModel>().Result!;
            model.MedicalConditions = GetMedicalConditions(userId);

            return View(model);
        }

        // Consulta las condiciones medicas del usuario
        private List<MedicalConditionModel> GetMedicalConditions(int userId)
        {
            using var client = CreateApiClient();

            var urlApi = _config["Valores:UrlApi"] + "User/GetMedicalConditionsAPI?UserId=" + userId;
            var response = client.GetAsync(urlApi).Result;

            if (response.StatusCode != HttpStatusCode.OK)
                return [];

            return response.Content.ReadFromJsonAsync<List<MedicalConditionModel>>().Result ?? [];
        }

        #endregion

        #region Actualizar Perfil

        [HttpPost]
        public IActionResult ActualizarPerfil(ProfileModel model)
        {
            model.UserId = HttpContext.Session.GetInt32("UserId")!.Value;

            using var client = CreateApiClient();

            var urlApi = _config["Valores:UrlApi"] + "User/UpdateProfileAPI";
            var response = client.PutAsJsonAsync(urlApi, model).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                // Se refresca el nombre mostrado en la barra superior
                HttpContext.Session.SetString("Name", model.Name);

                TempData["MensajePerfil"] = response.Content.ReadAsStringAsync().Result;
                TempData["ClaseMensajePerfil"] = "success";
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Salir", "Account");
            }
            else
            {
                TempData["MensajePerfil"] = response.Content.ReadAsStringAsync().Result;
                TempData["ClaseMensajePerfil"] = "danger";
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region Cambiar Contrasena

        [HttpPost]
        public IActionResult CambiarContrasena(ProfileModel model)
        {
            // Validar que las contrasenas coincidan
            if (model.Password != model.ConfirmPassword)
            {
                TempData["MensajeSeguridad"] = "Las contrasenas no coinciden.";
                TempData["ClaseMensajeSeguridad"] = "danger";
                return RedirectToAction("Index");
            }

            // Validar los requisitos minimos de seguridad
            if (!Regex.IsMatch(model.Password, PasswordPattern))
            {
                TempData["MensajeSeguridad"] = "La contrasena debe tener al menos 8 caracteres, incluyendo mayusculas, minusculas, un numero y un caracter especial.";
                TempData["ClaseMensajeSeguridad"] = "danger";
                return RedirectToAction("Index");
            }

            model.UserId = HttpContext.Session.GetInt32("UserId")!.Value;
            model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

            using var client = CreateApiClient();

            var urlApi = _config["Valores:UrlApi"] + "User/ChangePasswordAPI";
            var response = client.PutAsJsonAsync(urlApi, model).Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Salir", "Account");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                HttpContext.Session.Clear();

                TempData["MensajeExito"] = "Su contrasena se actualizo correctamente. Por favor inicie sesion nuevamente.";
                return RedirectToAction("Login", "Account");
            }

            TempData["MensajeSeguridad"] = response.Content.ReadAsStringAsync().Result;
            TempData["ClaseMensajeSeguridad"] = "danger";

            return RedirectToAction("Index");
        }

        #endregion

        #region Condiciones Medicas

        [HttpPost]
        public IActionResult AgregarCondicion(MedicalConditionModel model)
        {
            model.UserId = HttpContext.Session.GetInt32("UserId")!.Value;

            using var client = CreateApiClient();

            var urlApi = _config["Valores:UrlApi"] + "User/RegisterMedicalConditionAPI";
            var response = client.PostAsJsonAsync(urlApi, model).Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Salir", "Account");

            TempData["MensajeCondicion"] = response.Content.ReadAsStringAsync().Result;
            TempData["ClaseMensajeCondicion"] = response.StatusCode == HttpStatusCode.OK ? "success" : "danger";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult EliminarCondicion(int MedicalConditionId)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            using var client = CreateApiClient();

            var urlApi = _config["Valores:UrlApi"]
                + "User/DeleteMedicalConditionAPI?MedicalConditionId=" + MedicalConditionId
                + "&UserId=" + userId;
            var response = client.DeleteAsync(urlApi).Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Salir", "Account");

            TempData["MensajeCondicion"] = response.Content.ReadAsStringAsync().Result;
            TempData["ClaseMensajeCondicion"] = response.StatusCode == HttpStatusCode.OK ? "success" : "danger";

            return RedirectToAction("Index");
        }

        #endregion
    }
}
