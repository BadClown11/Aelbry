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

        [HttpGet]
        [Authorize(Policy = "Permission:USERS_VIEW")]
        public JsonResult GetByTeam(int teamId)
        {
            return Exec(() => _userBL.GetByTeam(teamId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:USERS_CREATE")]
        public JsonResult Create([FromBody] UserCreateRequest request)
        {
            var result = Exec(() =>
            {
                request.User.CreatedBy = CurrentUserId;
                return _userBL.Create(request.User, request.Password);
            });

            if (WasSuccessful(result))
            {
                Audit("USERS", "CREATE", request.User.UserId, dataBefore: null, dataAfter: AuditSnapshot(request.User));
            }

            return result;
        }

        [HttpPost]
        [Authorize(Policy = "Permission:USERS_EDIT")]
        public JsonResult Update([FromBody] User user)
        {
            var before = _userBL.GetById(user.UserId);

            var result = Exec(() =>
            {
                user.ModifiedBy = CurrentUserId;
                _userBL.Update(user);
            });

            if (WasSuccessful(result))
            {
                Audit("USERS", "UPDATE", user.UserId, AuditSnapshot(before), AuditSnapshot(user));
            }

            return result;
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
            var before = _userBL.GetById(userId);

            var result = Exec(() => _userBL.Delete(userId, CurrentUserId));

            if (WasSuccessful(result))
            {
                Audit("USERS", "DELETE", userId, AuditSnapshot(before), dataAfter: null);
            }

            return result;
        }

        // Nunca se persiste PasswordHash en la bitacora de auditoria, aunque sea un hash.
        private static object AuditSnapshot(User user)
        {
            if (user == null)
            {
                return null;
            }

            return new { user.UserId, user.CompanyId, user.Email, user.FirstName, user.LastName, user.JobTitle, user.IsActive };
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
