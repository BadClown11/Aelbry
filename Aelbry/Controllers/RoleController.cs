using Aelbry.BL;
using Aelbry.BO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    [Authorize(Policy = "Permission:ROLES_MANAGE")]
    public class RoleController : ApiControllerBase
    {
        private readonly RoleBL _roleBL;

        public RoleController(RoleBL roleBL)
        {
            _roleBL = roleBL;
        }

        [HttpGet]
        public JsonResult GetAll()
        {
            return Exec(() => _roleBL.GetAll());
        }

        [HttpGet]
        public JsonResult GetById(int roleId)
        {
            return Exec(() => _roleBL.GetById(roleId));
        }

        [HttpPost]
        public JsonResult Create([FromBody] Role role)
        {
            return Exec(() =>
            {
                role.CreatedBy = CurrentUserId;
                return _roleBL.Create(role);
            });
        }

        [HttpPost]
        public JsonResult Update([FromBody] Role role)
        {
            return Exec(() =>
            {
                role.ModifiedBy = CurrentUserId;
                _roleBL.Update(role);
            });
        }

        [HttpPost]
        public JsonResult Delete(int roleId)
        {
            return Exec(() => _roleBL.Delete(roleId, CurrentUserId));
        }

        [HttpGet]
        public JsonResult GetAllPermissions()
        {
            return Exec(() => _roleBL.GetAllPermissions());
        }

        [HttpPost]
        public JsonResult AssignPermission(int roleId, int permissionId)
        {
            return Exec(() => _roleBL.AssignPermission(roleId, permissionId));
        }

        [HttpPost]
        public JsonResult RemovePermission(int roleId, int permissionId)
        {
            return Exec(() => _roleBL.RemovePermission(roleId, permissionId));
        }
    }
}
