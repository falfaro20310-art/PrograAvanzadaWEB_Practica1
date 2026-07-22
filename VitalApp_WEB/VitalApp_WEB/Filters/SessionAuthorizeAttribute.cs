using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VitalApp_WEB.Filters
{
    // Protege las acciones que requieren una sesion activa
    public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var authenticated = context.HttpContext.Session.GetString("Authenticated");

            if (authenticated != "1")
            {
                context.Result = new RedirectToActionResult(
                    "Login",
                    "Account",
                    null);
            }
        }
    }
}
