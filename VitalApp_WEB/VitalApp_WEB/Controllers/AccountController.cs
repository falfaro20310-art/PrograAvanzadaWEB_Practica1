using Microsoft.AspNetCore.Mvc;
using System.Net;
using VitalApp_WEB.Filters;
using VitalApp_WEB.Models;

namespace VitalApp_WEB.Controllers
{
    public class AccountController(
        IHttpClientFactory _http,
        IConfiguration _config) : Controller
    {
        #region Iniciar Sesion

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(UserModel model)
        {
            using var client = _http.CreateClient();

            var urlApi = _config["Valores:UrlApi"] + "Home/LoginAPI";
            var response = client.PostAsJsonAsync(urlApi, model).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var datos = response.Content.ReadFromJsonAsync<UserModel>().Result;

                HttpContext.Session.SetString("Authenticated", "1");
                HttpContext.Session.SetInt32("UserId", datos!.UserId);
                HttpContext.Session.SetString("Name", datos!.Name);
                HttpContext.Session.SetString("Token", datos!.Token);
                HttpContext.Session.SetInt32("RoleId", datos!.RoleId);
                HttpContext.Session.SetString("RoleName", datos!.RoleName);

                return RedirectToAction("HomePage", "Home");
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                ViewBag.Mensaje = response.Content.ReadAsStringAsync().Result;
                return View();
            }

            throw new Exception("Ocurrio un error al intentar iniciar sesion.");
        }

        #endregion

        #region Cerrar Sesion

        [SessionAuthorize]
        [HttpGet]
        public IActionResult Salir()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        #endregion
    }
}
