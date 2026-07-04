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
            var result = Exec(() =>
            {
                project.CreatedBy = CurrentUserId;
                return _projectBL.Create(project);
            });

            if (WasSuccessful(result))
            {
                Audit("PROJECTS", "CREATE", project.ProjectId, dataBefore: null, dataAfter: project);
            }

            return result;
        }

        [HttpPost]
        [Authorize(Policy = "Permission:PROJECTS_EDIT")]
        public JsonResult Update([FromBody] Project project)
        {
            var before = _projectBL.GetById(project.ProjectId);

            var result = Exec(() =>
            {
                project.ModifiedBy = CurrentUserId;
                _projectBL.Update(project);
            });

            if (WasSuccessful(result))
            {
                Audit("PROJECTS", "UPDATE", project.ProjectId, before, project);
            }

            return result;
        }

        [HttpPost]
        [Authorize(Policy = "Permission:PROJECTS_DELETE")]
        public JsonResult Delete(int projectId)
        {
            var before = _projectBL.GetById(projectId);

            var result = Exec(() => _projectBL.Delete(projectId, CurrentUserId));

            if (WasSuccessful(result))
            {
                Audit("PROJECTS", "DELETE", projectId, before, dataAfter: null);
            }

            return result;
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

        /// <summary>
        /// Duplicacion profunda (Modulo 4): clona el proyecto, sus etiquetas y el arbol
        /// completo de actividades (subactividades, checklists, etiquetas de cada una).
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "Permission:PROJECTS_CREATE")]
        public JsonResult Duplicate(int projectId, string newCode, string newName)
        {
            return Exec(() => _projectBL.Duplicate(projectId, newCode, newName, CurrentUserId));
        }

        /// <summary>
        /// Aplica una plantilla corporativa (Modulo 4): crea el proyecto con los valores por
        /// defecto de la plantilla y clona su esqueleto de actividades.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "Permission:PROJECTS_CREATE")]
        public JsonResult CreateFromTemplate(int projectTemplateId, string code, string name, int companyId, int projectStatusId, int projectManagerId)
        {
            return Exec(() => _projectBL.CreateFromTemplate(projectTemplateId, code, name, companyId, projectStatusId, projectManagerId, CurrentUserId));
        }
    }
}
