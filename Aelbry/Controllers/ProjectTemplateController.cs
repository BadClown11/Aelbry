using Aelbry.BL;
using Aelbry.BO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    [Authorize]
    public class ProjectTemplateController : ApiControllerBase
    {
        private readonly ProjectTemplateBL _projectTemplateBL;

        public ProjectTemplateController(ProjectTemplateBL projectTemplateBL)
        {
            _projectTemplateBL = projectTemplateBL;
        }

        [HttpGet]
        [Authorize(Policy = "Permission:PROJECTS_VIEW")]
        public JsonResult GetByCompany(int companyId)
        {
            return Exec(() => _projectTemplateBL.GetByCompany(companyId));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:PROJECTS_VIEW")]
        public JsonResult GetById(int projectTemplateId)
        {
            return Exec(() => _projectTemplateBL.GetById(projectTemplateId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:PROJECT_TEMPLATES_MANAGE")]
        public JsonResult Create([FromBody] ProjectTemplate template)
        {
            return Exec(() =>
            {
                template.CreatedBy = CurrentUserId;
                return _projectTemplateBL.Create(template);
            });
        }

        [HttpPost]
        [Authorize(Policy = "Permission:PROJECT_TEMPLATES_MANAGE")]
        public JsonResult Update([FromBody] ProjectTemplate template)
        {
            return Exec(() =>
            {
                template.ModifiedBy = CurrentUserId;
                _projectTemplateBL.Update(template);
            });
        }

        [HttpPost]
        [Authorize(Policy = "Permission:PROJECT_TEMPLATES_MANAGE")]
        public JsonResult Delete(int projectTemplateId)
        {
            return Exec(() => _projectTemplateBL.Delete(projectTemplateId, CurrentUserId));
        }
    }
}
