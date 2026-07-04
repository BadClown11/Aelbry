using Aelbry.BL;
using Aelbry.BO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    [Authorize]
    public class TimeEntryController : ApiControllerBase
    {
        private readonly TimeEntryBL _timeEntryBL;

        public TimeEntryController(TimeEntryBL timeEntryBL)
        {
            _timeEntryBL = timeEntryBL;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Policy = "Permission:TIME_ENTRIES_MANAGE")]
        public JsonResult GetRunning()
        {
            return Exec(() => _timeEntryBL.GetRunningByUser(CurrentUserId));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:ACTIVITIES_VIEW")]
        public JsonResult GetByActivity(int activityId)
        {
            return Exec(() => _timeEntryBL.GetByActivity(activityId));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:TIME_ENTRIES_MANAGE")]
        public JsonResult GetMine(DateTime? startDate, DateTime? endDate)
        {
            return Exec(() => _timeEntryBL.GetByUser(CurrentUserId, startDate, endDate));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:TIME_ENTRIES_VIEW")]
        public JsonResult GetByUser(int userId, DateTime? startDate, DateTime? endDate)
        {
            return Exec(() => _timeEntryBL.GetByUser(userId, startDate, endDate));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:TIME_ENTRIES_MANAGE")]
        public JsonResult Start(int activityId)
        {
            return Exec(() => _timeEntryBL.Start(activityId, CurrentUserId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:TIME_ENTRIES_MANAGE")]
        public JsonResult Stop(int timeEntryId)
        {
            return Exec(() => _timeEntryBL.Stop(timeEntryId, CurrentUserId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:TIME_ENTRIES_MANAGE")]
        public JsonResult CreateManual([FromBody] TimeEntry entry)
        {
            return Exec(() => _timeEntryBL.CreateManual(entry, CurrentUserId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:TIME_ENTRIES_MANAGE")]
        public JsonResult Update(int timeEntryId, decimal durationHours, bool isOvertime, string notes)
        {
            return Exec(() => _timeEntryBL.Update(timeEntryId, durationHours, isOvertime, notes, CurrentUserId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:TIME_ENTRIES_MANAGE")]
        public JsonResult Delete(int timeEntryId)
        {
            return Exec(() => _timeEntryBL.Delete(timeEntryId, CurrentUserId));
        }
    }
}
