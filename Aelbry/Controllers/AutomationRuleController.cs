using Aelbry.BL;
using Aelbry.BO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    [Authorize]
    public class AutomationRuleController : ApiControllerBase
    {
        private readonly AutomationRuleBL _automationRuleBL;

        public AutomationRuleController(AutomationRuleBL automationRuleBL)
        {
            _automationRuleBL = automationRuleBL;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Policy = "Permission:AUTOMATION_RULES_VIEW")]
        public JsonResult GetByCompany(int companyId)
        {
            return Exec(() => _automationRuleBL.GetByCompany(companyId));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:AUTOMATION_RULES_VIEW")]
        public JsonResult GetById(int automationRuleId)
        {
            return Exec(() => _automationRuleBL.GetById(automationRuleId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:AUTOMATION_RULES_MANAGE")]
        public JsonResult Create([FromBody] AutomationRule rule)
        {
            return Exec(() =>
            {
                rule.CreatedBy = CurrentUserId;
                return _automationRuleBL.Create(rule);
            });
        }

        [HttpPost]
        [Authorize(Policy = "Permission:AUTOMATION_RULES_MANAGE")]
        public JsonResult Update([FromBody] AutomationRule rule)
        {
            return Exec(() =>
            {
                rule.ModifiedBy = CurrentUserId;
                _automationRuleBL.Update(rule);
            });
        }

        [HttpPost]
        [Authorize(Policy = "Permission:AUTOMATION_RULES_MANAGE")]
        public JsonResult Delete(int automationRuleId)
        {
            return Exec(() => _automationRuleBL.Delete(automationRuleId, CurrentUserId));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:AUTOMATION_RULES_VIEW")]
        public JsonResult GetLogs(int automationRuleId)
        {
            return Exec(() => _automationRuleBL.GetLogs(automationRuleId));
        }
    }
}
