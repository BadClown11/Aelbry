using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    /// <summary>
    /// Solo sirve la vista; los datos vienen de ActivityController (GetTreeByProject).
    /// </summary>
    [Authorize]
    public class CalendarViewController : Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }
    }
}
