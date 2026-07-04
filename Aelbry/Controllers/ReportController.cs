using Aelbry.BL;
using Aelbry.BO.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    [Authorize]
    public class ReportController : ApiControllerBase
    {
        private readonly ReportBL _reportBL;

        public ReportController(ReportBL reportBL)
        {
            _reportBL = reportBL;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Policy = "Permission:REPORTS_VIEW")]
        public JsonResult GetWeeklyReport(int companyId, int? projectId, int? userId, int? teamId, int? departmentId, DateTime? startDate, DateTime? endDate)
        {
            var filter = new ReportFilter
            {
                CompanyId = companyId,
                ProjectId = projectId,
                UserId = userId,
                TeamId = teamId,
                DepartmentId = departmentId,
                StartDate = startDate,
                EndDate = endDate,
            };

            return Exec(() => _reportBL.GetWeeklyReport(filter));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:REPORTS_VIEW")]
        public JsonResult GetKpiSummary(int companyId, int? projectId, int? userId, int? teamId, int? departmentId, DateTime weekStart)
        {
            var filter = new ReportFilter { CompanyId = companyId, ProjectId = projectId, UserId = userId, TeamId = teamId, DepartmentId = departmentId };

            return Exec(() => _reportBL.GetKpiSummary(filter, weekStart));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:REPORTS_VIEW")]
        public JsonResult GetProgressByUser(int companyId, int? projectId, int? userId, int? teamId, int? departmentId)
        {
            var filter = new ReportFilter { CompanyId = companyId, ProjectId = projectId, UserId = userId, TeamId = teamId, DepartmentId = departmentId };

            return Exec(() => _reportBL.GetProgressByUser(filter));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:REPORTS_VIEW")]
        public JsonResult GetPriorityDistribution(int companyId, int? projectId, int? userId, int? teamId, int? departmentId)
        {
            var filter = new ReportFilter { CompanyId = companyId, ProjectId = projectId, UserId = userId, TeamId = teamId, DepartmentId = departmentId };

            return Exec(() => _reportBL.GetPriorityDistribution(filter));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:REPORTS_VIEW")]
        public JsonResult GetBurndown(int companyId, int projectId, DateTime startDate, DateTime endDate)
        {
            return Exec(() => _reportBL.GetBurndown(companyId, projectId, startDate, endDate));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:REPORTS_VIEW")]
        public IActionResult ExportExcel(int companyId, int? projectId, int? userId, int? teamId, int? departmentId, DateTime? startDate, DateTime? endDate)
        {
            var filter = BuildFilter(companyId, projectId, userId, teamId, departmentId, startDate, endDate);
            var bytes = _reportBL.ExportWeeklyReportToExcel(filter, "Reporte semanal de actividades");
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ReporteSemanal.xlsx");
        }

        [HttpGet]
        [Authorize(Policy = "Permission:REPORTS_VIEW")]
        public IActionResult ExportWord(int companyId, int? projectId, int? userId, int? teamId, int? departmentId, DateTime? startDate, DateTime? endDate)
        {
            var filter = BuildFilter(companyId, projectId, userId, teamId, departmentId, startDate, endDate);
            var bytes = _reportBL.ExportWeeklyReportToWord(filter, "Reporte semanal de actividades");
            return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "ReporteSemanal.docx");
        }

        [HttpGet]
        [Authorize(Policy = "Permission:REPORTS_VIEW")]
        public IActionResult ExportPdf(int companyId, int? projectId, int? userId, int? teamId, int? departmentId, DateTime? startDate, DateTime? endDate)
        {
            var filter = BuildFilter(companyId, projectId, userId, teamId, departmentId, startDate, endDate);
            var bytes = _reportBL.ExportWeeklyReportToPdf(filter, "Reporte semanal de actividades");
            return File(bytes, "application/pdf", "ReporteSemanal.pdf");
        }

        private static ReportFilter BuildFilter(int companyId, int? projectId, int? userId, int? teamId, int? departmentId, DateTime? startDate, DateTime? endDate)
        {
            return new ReportFilter
            {
                CompanyId = companyId,
                ProjectId = projectId,
                UserId = userId,
                TeamId = teamId,
                DepartmentId = departmentId,
                StartDate = startDate,
                EndDate = endDate,
            };
        }
    }
}
