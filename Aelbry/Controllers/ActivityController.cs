using Aelbry.BL;
using Aelbry.BO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    [Authorize]
    public class ActivityController : ApiControllerBase
    {
        private readonly ActivityBL _activityBL;

        public ActivityController(ActivityBL activityBL)
        {
            _activityBL = activityBL;
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
