using Microsoft.AspNetCore.Mvc;

namespace VitalApp_WEB.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
