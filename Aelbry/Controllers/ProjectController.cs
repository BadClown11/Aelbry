using Aelbry.BL;
using Aelbry.BO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    [Authorize]
    public class ProjectController : ApiControllerBase
    {
        private readonly ProjectBL _projectBL;

        public ProjectController(ProjectBL projectBL)
        {
            _projectBL = projectBL;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Policy = "Permission:PROJECTS_VIEW")]
        public JsonResult GetByCompany(int companyId)
        {
            return Exec(() => _projectBL.GetByCompany(companyId));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:PROJECTS_VIEW")]
        public JsonResult GetById(int projectId)
        {
            return Exec(() => _projectBL.GetById(projectId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:PROJECTS_CREATE")]
        public JsonResult Create([FromBody] Project project)
        {
            return Exec(() =>
            {
                project.CreatedBy = CurrentUserId;
                return _projectBL.Create(project);
            });
        }

        [HttpPost]
        [Authorize(Policy = "Permission:PROJECTS_EDIT")]
        public JsonResult Update([FromBody] Project project)
        {
            return Exec(() =>
            {
                project.ModifiedBy = CurrentUserId;
                _projectBL.Update(project);
            });
        }

        [HttpPost]
        [Authorize(Policy = "Permission:PROJECTS_DELETE")]
        public JsonResult Delete(int projectId)
        {
            return Exec(() => _projectBL.Delete(projectId, CurrentUserId));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:PROJECTS_VIEW")]
        public JsonResult GetMembers(int projectId)
        {
            return Exec(() => _projectBL.GetMembers(projectId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:PROJECT_MEMBERS_MANAGE")]
        public JsonResult AddMember(int projectId, int userId)
        {
            return Exec(() => _projectBL.AddMember(projectId, userId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:PROJECT_MEMBERS_MANAGE")]
        public JsonResult RemoveMember(int projectId, int userId)
        {
            return Exec(() => _projectBL.RemoveMember(projectId, userId));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:PROJECTS_VIEW")]
        public JsonResult GetTags(int projectId)
        {
            return Exec(() => _projectBL.GetTags(projectId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:PROJECTS_EDIT")]
        public JsonResult AddTag(int projectId, int tagId)
        {
            return Exec(() => _projectBL.AddTag(projectId, tagId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:PROJECTS_EDIT")]
        public JsonResult RemoveTag(int projectId, int tagId)
        {
            return Exec(() => _projectBL.RemoveTag(projectId, tagId));
        }
    }
}
