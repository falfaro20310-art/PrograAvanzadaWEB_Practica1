using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VitalApp_WEB.Filters
{
    // Protege acciones que solo un doctor puede usar
    public class DoctorAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var session = context.HttpContext.Session;

            // Sin sesion activa vuelve al login
            if (session.GetString("Authenticated") != "1")
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // Autenticado pero sin rol de doctor vuelve al dashboard
            if (session.GetString("RoleName") != "Doctor")
            {
                context.Result = new RedirectToActionResult("HomePage", "Home", null);
            }
        }
    }
}
