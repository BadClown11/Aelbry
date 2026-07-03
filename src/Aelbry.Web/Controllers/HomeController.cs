using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        [Route("/Home/Error")]
        public IActionResult Error()
        {
            return View();
        }
    }
}
