using Aelbry.BL;
using Aelbry.BO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    [Authorize]
    public class UserController : ApiControllerBase
    {
        private readonly UserBL _userBL;

        public UserController(UserBL userBL)
        {
            _userBL = userBL;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Policy = "Permission:USERS_VIEW")]
        public JsonResult GetByCompany(int companyId)
        {
            return Exec(() => _userBL.GetByCompany(companyId));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:USERS_VIEW")]
        public JsonResult GetById(int userId)
        {
            return Exec(() => _userBL.GetById(userId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:USERS_CREATE")]
        public JsonResult Create([FromBody] UserCreateRequest request)
        {
            return Exec(() =>
            {
                request.User.CreatedBy = CurrentUserId;
                return _userBL.Create(request.User, request.Password);
            });
        }

        [HttpPost]
        [Authorize(Policy = "Permission:USERS_EDIT")]
        public JsonResult Update([FromBody] User user)
        {
            return Exec(() =>
            {
                user.ModifiedBy = CurrentUserId;
                _userBL.Update(user);
            });
        }

        [HttpPost]
        public JsonResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            return Exec(() => _userBL.ChangePassword(CurrentUserId, request.CurrentPassword, request.NewPassword, CurrentUserId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:USERS_DELETE")]
        public JsonResult Delete(int userId)
        {
            return Exec(() => _userBL.Delete(userId, CurrentUserId));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:USERS_VIEW")]
        public JsonResult GetRoles(int userId)
        {
            return Exec(() => _userBL.GetRoles(userId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:ROLES_MANAGE")]
        public JsonResult AssignRole([FromBody] AssignRoleRequest request)
        {
            return Exec(() => _userBL.AssignRole(request.UserId, request.RoleId, request.CompanyId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:ROLES_MANAGE")]
        public JsonResult RemoveRole([FromBody] AssignRoleRequest request)
        {
            return Exec(() => _userBL.RemoveRole(request.UserId, request.RoleId, request.CompanyId));
        }
    }
}
