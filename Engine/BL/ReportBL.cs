using Aelbry.BL.Export;
using Aelbry.BO;
using Aelbry.BO.Reports;
using Aelbry.DAL;

namespace Aelbry.BL
{
    /// <summary>
    /// Orquesta el Modulo 8 (parte 1): reporte semanal, KPIs, graficas de avance/prioridad y
    /// burndown. Todos parten de la misma consulta filtrable (SP_REPORT_GET_ACTIVITIES); la
    /// agregacion/calculo se hace aqui en C# sobre ese dataset.
    /// </summary>
    public class ReportBL
    {
        public List<ReportActivityRow> GetWeeklyReport(ReportFilter filter)
        {
            using (var dal = ReportDAL.Instance)
            {
                var rows = dal.GetActivities(filter);
                var today = DateTime.UtcNow.Date;

                foreach (var row in rows)
                {
                    if (row.EstimatedStartDate.HasValue)
                    {
                        row.DaysElapsed = Math.Max(0, (today - row.EstimatedStartDate.Value.Date).Days);
                    }

                    if (row.EstimatedEndDate.HasValue)
                    {
                        row.DaysRemaining = (row.EstimatedEndDate.Value.Date - today).Days;
                        // En riesgo: ya vencio (fecha estimada de fin en el pasado) y sigue sin completarse/cancelarse.
                        row.IsAtRisk = row.Status != ActivityStatus.Completed
                            && row.Status != ActivityStatus.Cancelled
                            && row.DaysRemaining.Value < 0;
                    }
                }

                return rows;
            }
        }

        public KpiSummary GetKpiSummary(ReportFilter filter, DateTime weekStart)
        {
            using (var dal = ReportDAL.Instance)
            {
                var snapshotFilter = CloneWithoutDateRange(filter);
                var allActivities = dal.GetActivities(snapshotFilter);

                var summary = new KpiSummary
                {
                    CountsByStatus = allActivities
                        .GroupBy(a => a.Status)
                        .ToDictionary(g => g.Key, g => g.Count()),
                };

                DateTime thisWeekStart = weekStart.Date;
                DateTime thisWeekEnd = thisWeekStart.AddDays(6);
                DateTime lastWeekStart = thisWeekStart.AddDays(-7);
                DateTime lastWeekEnd = thisWeekStart.AddDays(-1);

                summary.DueThisWeek = allActivities.Count(a => IsWithin(a.EstimatedEndDate, thisWeekStart, thisWeekEnd));
                summary.CompletedThisWeek = allActivities.Count(a => a.Status == ActivityStatus.Completed && IsWithin(a.ActualEndDate, thisWeekStart, thisWeekEnd));
                summary.ProductivityThisWeek = ReportKpiCalculator.CalculateProductivityPercent(summary.CompletedThisWeek, summary.DueThisWeek);

                summary.DueLastWeek = allActivities.Count(a => IsWithin(a.EstimatedEndDate, lastWeekStart, lastWeekEnd));
                summary.CompletedLastWeek = allActivities.Count(a => a.Status == ActivityStatus.Completed && IsWithin(a.ActualEndDate, lastWeekStart, lastWeekEnd));
                summary.ProductivityLastWeek = ReportKpiCalculator.CalculateProductivityPercent(summary.CompletedLastWeek, summary.DueLastWeek);

                summary.ProductivityDelta = summary.ProductivityThisWeek - summary.ProductivityLastWeek;

                return summary;
            }
        }

        public List<UserProgressPoint> GetProgressByUser(ReportFilter filter)
        {
            using (var dal = ReportDAL.Instance)
            {
                var rows = dal.GetActivities(CloneWithoutDateRange(filter));

                return rows
                    .GroupBy(a => new { a.ResponsibleUserId, a.ResponsibleName })
                    .Select(g => new UserProgressPoint
                    {
                        UserId = g.Key.ResponsibleUserId,
                        UserName = g.Key.ResponsibleName,
                        ActivityCount = g.Count(),
                        AverageProgress = Math.Round(g.Average(a => a.ProgressPercentage), 2),
                    })
                    .OrderByDescending(p => p.AverageProgress)
                    .ToList();
            }
        }

        public List<PriorityDistributionPoint> GetPriorityDistribution(ReportFilter filter)
        {
            using (var dal = ReportDAL.Instance)
            {
                var rows = dal.GetActivities(CloneWithoutDateRange(filter));

                return rows
                    .GroupBy(a => a.Priority)
                    .Select(g => new PriorityDistributionPoint { Priority = g.Key, Count = g.Count() })
                    .OrderBy(p => p.Priority)
                    .ToList();
            }
        }

        public List<BurndownPoint> GetBurndown(int companyId, int projectId, DateTime startDate, DateTime endDate)
        {
            using (var dal = ReportDAL.Instance)
            {
                var filter = new ReportFilter { CompanyId = companyId, ProjectId = projectId };
                var rows = dal.GetActivities(filter);

                var snapshots = rows.Select(a => new BurndownCalculator.ActivitySnapshot
                {
                    EstimatedHours = a.EstimatedHours,
                    IsCompleted = a.Status == ActivityStatus.Completed,
                    ActualEndDate = a.ActualEndDate,
                }).ToList();

                return BurndownCalculator.Calculate(snapshots, startDate, endDate);
            }
        }

        public byte[] ExportWeeklyReportToExcel(ReportFilter filter, string title)
        {
            return ExcelReportExporter.Export(GetWeeklyReport(filter), title);
        }

        public byte[] ExportWeeklyReportToWord(ReportFilter filter, string title)
        {
            return WordReportExporter.Export(GetWeeklyReport(filter), title);
        }

        public byte[] ExportWeeklyReportToPdf(ReportFilter filter, string title)
        {
            return PdfReportExporter.Export(GetWeeklyReport(filter), title);
        }

        private static bool IsWithin(DateTime? date, DateTime start, DateTime end)
        {
            return date.HasValue && date.Value.Date >= start.Date && date.Value.Date <= end.Date;
        }

        private static ReportFilter CloneWithoutDateRange(ReportFilter filter)
        {
            return new ReportFilter
            {
                CompanyId = filter.CompanyId,
                ProjectId = filter.ProjectId,
                UserId = filter.UserId,
                TeamId = filter.TeamId,
                DepartmentId = filter.DepartmentId,
            };
        }
    }
}
