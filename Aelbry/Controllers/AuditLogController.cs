using Aelbry.BL;
using Aelbry.BO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    [Authorize]
    public class AuditLogController : ApiControllerBase
    {
        private readonly AuditLogBL _auditLogBL;

        public AuditLogController(AuditLogBL auditLogBL)
        {
            _auditLogBL = auditLogBL;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Policy = "Permission:AUDIT_VIEW")]
        public JsonResult GetByCompany(int companyId, string module, int? userId, DateTime? startDate, DateTime? endDate)
        {
            var filter = new AuditLogFilter
            {
                CompanyId = companyId,
                Module = module,
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate,
            };

            return Exec(() => _auditLogBL.GetByCompany(filter));
        }
    }
}
