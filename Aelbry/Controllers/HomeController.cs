using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            // El Home nunca "trata sobre" un solo proyecto (muestra el feed de actividades
            // completo, sin importar cual sea el proyecto activo), asi que el encabezado
            // generico "Selecciona un proyecto" del shell no aplica aqui.
            ViewData["HideProjectHeader"] = true;
            return View();
        }

        [Route("/Home/Error")]
        public IActionResult Error()
        {
            return View();
        }
    }
}
