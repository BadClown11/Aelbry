using Aelbry.BL;
using Aelbry.BO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    [Authorize]
    public class ActivityCategoryController : ApiControllerBase
    {
        private readonly ActivityCategoryBL _activityCategoryBL;

        public ActivityCategoryController(ActivityCategoryBL activityCategoryBL)
        {
            _activityCategoryBL = activityCategoryBL;
        }

        [HttpGet]
        [Authorize(Policy = "Permission:ACTIVITIES_VIEW")]
        public JsonResult GetByCompany(int companyId)
        {
            return Exec(() => _activityCategoryBL.GetByCompany(companyId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:ACTIVITIES_EDIT")]
        public JsonResult Create([FromBody] ActivityCategory category)
        {
            return Exec(() =>
            {
                category.CreatedBy = CurrentUserId;
                return _activityCategoryBL.Create(category);
            });
        }
    }
}
