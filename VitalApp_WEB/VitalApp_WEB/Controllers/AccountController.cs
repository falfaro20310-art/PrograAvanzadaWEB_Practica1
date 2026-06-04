using Microsoft.AspNetCore.Mvc;

namespace VitalApp_WEB.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}
