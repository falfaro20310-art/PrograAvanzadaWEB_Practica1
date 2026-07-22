using Microsoft.AspNetCore.Mvc;
using System.Net;
using VitalApp_WEB.Models;

namespace VitalApp_WEB.Controllers
{
    public class RecuperarContrasenaController(
        IHttpClientFactory _http,
        IConfiguration _config) : Controller
    {
        #region Recuperar Contrasena

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(UserModel model)
        {
            using var client = _http.CreateClient();

            var urlApi = _config["Valores:UrlApi"] + "Home/RecoverPasswordAPI";
            var response = client.PostAsJsonAsync(urlApi, model).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                TempData["MensajeExito"] = "Te enviamos una contraseña temporal a tu correo electrónico.";
                return RedirectToAction("Login", "Account");
            }
            else if (response.StatusCode == HttpStatusCode.NotFound
                  || response.StatusCode == HttpStatusCode.BadRequest)
            {
                ViewBag.Mensaje = response.Content.ReadAsStringAsync().Result;
                return View(model);
            }

            throw new Exception("Ocurrio un error al intentar recuperar su acceso.");
        }

        #endregion
    }
}
