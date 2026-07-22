using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using VitalApp_WEB.Filters;
using VitalApp_WEB.Models;

namespace VitalApp_WEB.Controllers
{
    public class HomeController(
        IHttpClientFactory _http,
        IConfiguration _config) : Controller
    {
        // Expresion para validar la seguridad minima de la contrasena
        private const string PasswordPattern =
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$";

        public IActionResult Index()
        {
            return RedirectToAction("Login", "Account");
        }

        [SessionAuthorize]
        public IActionResult HomePage()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        #region Registrar Usuarios

        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registro(UserModel model)
        {
            // Validar que las contrasenas coincidan
            if (model.Password != model.ConfirmPassword)
            {
                ViewBag.Mensaje = "Las contrasenas no coinciden.";
                return View(model);
            }

            // Validar los requisitos minimos de seguridad
            if (!Regex.IsMatch(model.Password, PasswordPattern))
            {
                ViewBag.Mensaje = "La contrasena debe tener al menos 8 caracteres, incluyendo mayusculas, minusculas, un numero y un caracter especial.";
                return View(model);
            }

            model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

            using var client = _http.CreateClient();
            var urlApi = _config["Valores:UrlApi"] + "Home/RegisterAPI";
            var response = client.PostAsJsonAsync(urlApi, model).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return RedirectToAction("Login", "Account");
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                ViewBag.Mensaje = response.Content.ReadAsStringAsync().Result;
                return View(model);
            }

            throw new Exception("Ocurrio un error al intentar registrar el usuario.");
        }

        #endregion

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
