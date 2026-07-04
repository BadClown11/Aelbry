using Aelbry.BL;
using Aelbry.BO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    [Authorize]
    public class ProjectStatusController : ApiControllerBase
    {
        private readonly ProjectStatusBL _projectStatusBL;

        public ProjectStatusController(ProjectStatusBL projectStatusBL)
        {
            _projectStatusBL = projectStatusBL;
        }

        [HttpGet]
        [Authorize(Policy = "Permission:PROJECTS_VIEW")]
        public JsonResult GetByCompany(int companyId)
        {
            return Exec(() => _projectStatusBL.GetByCompany(companyId));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:PROJECTS_VIEW")]
        public JsonResult GetById(int projectStatusId)
        {
            return Exec(() => _projectStatusBL.GetById(projectStatusId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:PROJECT_STATUSES_MANAGE")]
        public JsonResult Create([FromBody] ProjectStatus status)
        {
            return Exec(() =>
            {
                status.CreatedBy = CurrentUserId;
                return _projectStatusBL.Create(status);
            });
        }

        [HttpPost]
        [Authorize(Policy = "Permission:PROJECT_STATUSES_MANAGE")]
        public JsonResult Update([FromBody] ProjectStatus status)
        {
            return Exec(() =>
            {
                status.ModifiedBy = CurrentUserId;
                _projectStatusBL.Update(status);
            });
        }

        [HttpPost]
        [Authorize(Policy = "Permission:PROJECT_STATUSES_MANAGE")]
        public JsonResult Delete(int projectStatusId)
        {
            return Exec(() => _projectStatusBL.Delete(projectStatusId, CurrentUserId));
        }
    }
}
