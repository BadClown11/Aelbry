using Aelbry.BL;
using Aelbry.BO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    [Authorize(Policy = "Permission:DEPARTMENTS_MANAGE")]
    public class DepartmentController : ApiControllerBase
    {
        private readonly DepartmentBL _departmentBL;

        public DepartmentController(DepartmentBL departmentBL)
        {
            _departmentBL = departmentBL;
        }

        [HttpGet]
        public JsonResult GetByCompany(int companyId)
        {
            return Exec(() => _departmentBL.GetByCompany(companyId));
        }

        [HttpGet]
        public JsonResult GetById(int departmentId)
        {
            return Exec(() => _departmentBL.GetById(departmentId));
        }

        [HttpPost]
        public JsonResult Create([FromBody] Department department)
        {
            return Exec(() =>
            {
                department.CreatedBy = CurrentUserId;
                return _departmentBL.Create(department);
            });
        }

        [HttpPost]
        public JsonResult Update([FromBody] Department department)
        {
            return Exec(() =>
            {
                department.ModifiedBy = CurrentUserId;
                _departmentBL.Update(department);
            });
        }

        [HttpPost]
        public JsonResult Delete(int departmentId)
        {
            return Exec(() => _departmentBL.Delete(departmentId, CurrentUserId));
        }
    }
}
