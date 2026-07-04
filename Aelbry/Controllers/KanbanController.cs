using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    /// <summary>
    /// Solo sirve la vista; los datos vienen de los endpoints ya existentes en ActivityController
    /// (GetTreeByProject, UpdateStatus) para no duplicar API.
    /// </summary>
    [Authorize]
    public class KanbanController : Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }
    }
}
