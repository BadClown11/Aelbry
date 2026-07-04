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
            var result = Exec(() =>
            {
                role.CreatedBy = CurrentUserId;
                return _roleBL.Create(role);
            });

            if (WasSuccessful(result))
            {
                Audit("ROLES", "CREATE", role.RoleId, dataBefore: null, dataAfter: role);
            }

            return result;
        }

        [HttpPost]
        public JsonResult Update([FromBody] Role role)
        {
            var before = _roleBL.GetById(role.RoleId);

            var result = Exec(() =>
            {
                role.ModifiedBy = CurrentUserId;
                _roleBL.Update(role);
            });

            if (WasSuccessful(result))
            {
                Audit("ROLES", "UPDATE", role.RoleId, before, role);
            }

            return result;
        }

        [HttpPost]
        public JsonResult Delete(int roleId)
        {
            var before = _roleBL.GetById(roleId);

            var result = Exec(() => _roleBL.Delete(roleId, CurrentUserId));

            if (WasSuccessful(result))
            {
                Audit("ROLES", "DELETE", roleId, before, dataAfter: null);
            }

            return result;
        }

        [HttpGet]
        public JsonResult GetAllPermissions()
        {
            return Exec(() => _roleBL.GetAllPermissions());
        }

        [HttpPost]
        public JsonResult AssignPermission(int roleId, int permissionId)
        {
            var result = Exec(() => _roleBL.AssignPermission(roleId, permissionId));

            if (WasSuccessful(result))
            {
                Audit("PERMISSIONS", "ASSIGN", roleId, dataBefore: null, dataAfter: new { roleId, permissionId });
            }

            return result;
        }

        [HttpPost]
        public JsonResult RemovePermission(int roleId, int permissionId)
        {
            var result = Exec(() => _roleBL.RemovePermission(roleId, permissionId));

            if (WasSuccessful(result))
            {
                Audit("PERMISSIONS", "REMOVE", roleId, dataBefore: new { roleId, permissionId }, dataAfter: null);
            }

            return result;
        }
    }
}
