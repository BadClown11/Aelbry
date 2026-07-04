using Aelbry.BL;
using Aelbry.BL.Import;
using Aelbry.BO;
using Aelbry.BO.Import;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    [Authorize]
    public class ActivityController : ApiControllerBase
    {
        private readonly ActivityBL _activityBL;
        private readonly ExcelActivityImportBL _excelImportBL;

        public ActivityController(ActivityBL activityBL, ExcelActivityImportBL excelImportBL)
        {
            _activityBL = activityBL;
            _excelImportBL = excelImportBL;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Policy = "Permission:ACTIVITIES_VIEW")]
        public JsonResult GetTreeByProject(int projectId)
        {
            return Exec(() => _activityBL.GetTreeByProject(projectId));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:ACTIVITIES_VIEW")]
        public JsonResult GetById(int activityId)
        {
            return Exec(() => _activityBL.GetById(activityId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:ACTIVITIES_CREATE")]
        public JsonResult Create([FromBody] Activity activity)
        {
            return Exec(() =>
            {
                activity.CreatedBy = CurrentUserId;
                return _activityBL.Create(activity);
            });
        }

        [HttpPost]
        [Authorize(Policy = "Permission:ACTIVITIES_EDIT")]
        public JsonResult Update([FromBody] Activity activity)
        {
            return Exec(() =>
            {
                activity.ModifiedBy = CurrentUserId;
                _activityBL.Update(activity);
            });
        }

        [HttpPost]
        [Authorize(Policy = "Permission:ACTIVITIES_DELETE")]
        public JsonResult Delete(int activityId)
        {
            return Exec(() => _activityBL.Delete(activityId, CurrentUserId));
        }

        /// <summary>
        /// Creacion masiva por texto rapido (Modulo 4): una actividad por cada linea no vacia.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "Permission:ACTIVITIES_CREATE")]
        public JsonResult BulkCreate([FromBody] BulkCreateActivitiesRequest request)
        {
            return Exec(() => _activityBL.BulkCreate(request.ProjectId, request.ParentActivityId, request.Lines, CurrentUserId));
        }

        /// <summary>
        /// Duplicacion profunda de una actividad (Modulo 4): clona su subarbol completo
        /// (subactividades, checklist, etiquetas).
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "Permission:ACTIVITIES_CREATE")]
        public JsonResult Duplicate(int activityId, int? targetProjectId, int? targetParentActivityId)
        {
            return Exec(() => _activityBL.DuplicateActivity(activityId, targetProjectId, targetParentActivityId, CurrentUserId));
        }

        /// <summary>
        /// Paso 1 de la importacion masiva desde Excel: lee el archivo, cachea las filas bajo
        /// un token efimero, y devuelve las columnas detectadas para que el usuario las mapee.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "Permission:ACTIVITIES_CREATE")]
        public JsonResult ImportExcelPreview(IFormFile file)
        {
            return Exec(() =>
            {
                using var stream = file.OpenReadStream();
                return _excelImportBL.Preview(stream);
            });
        }

        /// <summary>
        /// Paso 2: aplica el mapeo de columnas elegido y crea una actividad por fila valida.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "Permission:ACTIVITIES_CREATE")]
        public JsonResult ImportExcelCommit([FromBody] ExcelImportCommitRequest request)
        {
            return Exec(() => _excelImportBL.Commit(request, CurrentUserId));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:ACTIVITIES_VIEW")]
        public JsonResult GetParticipants(int activityId)
        {
            return Exec(() => _activityBL.GetParticipants(activityId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:ACTIVITY_PARTICIPANTS_MANAGE")]
        public JsonResult AddParticipant(int activityId, int userId)
        {
            return Exec(() => _activityBL.AddParticipant(activityId, userId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:ACTIVITY_PARTICIPANTS_MANAGE")]
        public JsonResult RemoveParticipant(int activityId, int userId)
        {
            return Exec(() => _activityBL.RemoveParticipant(activityId, userId));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:ACTIVITIES_VIEW")]
        public JsonResult GetTags(int activityId)
        {
            return Exec(() => _activityBL.GetTags(activityId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:ACTIVITIES_EDIT")]
        public JsonResult AddTag(int activityId, int tagId)
        {
            return Exec(() => _activityBL.AddTag(activityId, tagId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:ACTIVITIES_EDIT")]
        public JsonResult RemoveTag(int activityId, int tagId)
        {
            return Exec(() => _activityBL.RemoveTag(activityId, tagId));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:ACTIVITIES_VIEW")]
        public JsonResult GetChecklistItems(int activityId)
        {
            return Exec(() => _activityBL.GetChecklistItems(activityId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:ACTIVITY_CHECKLISTS_MANAGE")]
        public JsonResult AddChecklistItem(int activityId, string text, int sequence)
        {
            return Exec(() => _activityBL.AddChecklistItem(activityId, text, sequence, CurrentUserId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:ACTIVITY_CHECKLISTS_MANAGE")]
        public JsonResult ToggleChecklistItem(int checklistItemId, int activityId, bool isChecked)
        {
            return Exec(() => _activityBL.ToggleChecklistItem(checklistItemId, activityId, isChecked, CurrentUserId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:ACTIVITY_CHECKLISTS_MANAGE")]
        public JsonResult DeleteChecklistItem(int checklistItemId, int activityId)
        {
            return Exec(() => _activityBL.DeleteChecklistItem(checklistItemId, activityId, CurrentUserId));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:ACTIVITIES_VIEW")]
        public JsonResult GetDependencies(int activityId)
        {
            return Exec(() => _activityBL.GetDependencies(activityId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:ACTIVITY_DEPENDENCIES_MANAGE")]
        public JsonResult AddDependency(int activityId, int dependsOnActivityId, DependencyType dependencyType)
        {
            return Exec(() => _activityBL.AddDependency(activityId, dependsOnActivityId, dependencyType, CurrentUserId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:ACTIVITY_DEPENDENCIES_MANAGE")]
        public JsonResult RemoveDependency(int activityDependencyId)
        {
            return Exec(() => _activityBL.RemoveDependency(activityDependencyId));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:ACTIVITIES_VIEW")]
        public JsonResult GetUserProgress(int userId)
        {
            return Exec(() => _activityBL.GetUserProgress(userId));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:ACTIVITIES_VIEW")]
        public JsonResult GetTeamProgress(int teamId)
        {
            return Exec(() => _activityBL.GetTeamProgress(teamId));
        }
    }
}
