using Aelbry.BL;
using Aelbry.BO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    [Authorize]
    public class TagController : ApiControllerBase
    {
        private readonly TagBL _tagBL;

        public TagController(TagBL tagBL)
        {
            _tagBL = tagBL;
        }

        [HttpGet]
        [Authorize(Policy = "Permission:PROJECTS_VIEW")]
        public JsonResult GetByCompany(int companyId)
        {
            return Exec(() => _tagBL.GetByCompany(companyId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:PROJECT_TAGS_MANAGE")]
        public JsonResult Create([FromBody] Tag tag)
        {
            return Exec(() =>
            {
                tag.CreatedBy = CurrentUserId;
                return _tagBL.Create(tag);
            });
        }

        [HttpPost]
        [Authorize(Policy = "Permission:PROJECT_TAGS_MANAGE")]
        public JsonResult Update([FromBody] Tag tag)
        {
            return Exec(() =>
            {
                tag.ModifiedBy = CurrentUserId;
                _tagBL.Update(tag);
            });
        }

        [HttpPost]
        [Authorize(Policy = "Permission:PROJECT_TAGS_MANAGE")]
        public JsonResult Delete(int tagId)
        {
            return Exec(() => _tagBL.Delete(tagId, CurrentUserId));
        }
    }
}
