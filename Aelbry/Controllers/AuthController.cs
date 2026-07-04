using Aelbry.BL;
using Aelbry.BO.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    public class AuthController : ApiControllerBase
    {
        private readonly AuthBL _authBL;

        public AuthController(AuthBL authBL)
        {
            _authBL = authBL;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult Login([FromBody] LoginRequest request)
        {
            return Exec(() => _authBL.Login(request.Email, request.Password, ClientIp));
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult Refresh([FromBody] RefreshRequest request)
        {
            return Exec(() => _authBL.Refresh(request.RefreshToken, ClientIp));
        }

        [HttpPost]
        [Authorize]
        public JsonResult Logout([FromBody] RefreshRequest request)
        {
            return Exec(() => _authBL.Logout(request.RefreshToken, ClientIp));
        }
    }
}
