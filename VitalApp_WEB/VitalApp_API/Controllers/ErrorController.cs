using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace VitalApp_API.Controllers
{
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]")]
    [ApiController]
    public class ErrorController : ControllerBase
    {
        // Captura las excepciones no controladas de la aplicacion
        [Route("RegisterError")]
        public IActionResult RegisterError()
        {
            var exception = HttpContext.Features.Get<IExceptionHandlerFeature>();

            return StatusCode(500, "Se presento un inconveniente tecnico");
        }
    }
}
