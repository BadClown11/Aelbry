using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    /// <summary>
    /// Solo sirve la vista; los datos vienen de ActivityController (GetTreeByProject,
    /// GetCriticalPath, UpdateDates) para no duplicar API.
    /// </summary>
    [Authorize]
    public class GanttController : Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }
    }
}
