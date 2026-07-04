namespace Aelbry.BO.Reports
{
    /// <summary>
    /// Fila cruda para reportes (viene de SP_REPORT_GET_ACTIVITIES). DaysElapsed/DaysRemaining/
    /// IsAtRisk los calcula ReportBL solo para el reporte semanal (quedan en 0/false en el resto).
    /// </summary>
    public class ReportActivityRow
    {
        public int ActivityId { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int ProjectId { get; set; }

        public string ProjectName { get; set; }

        public int ResponsibleUserId { get; set; }

        public string ResponsibleName { get; set; }

        public int? TeamId { get; set; }

        public string TeamName { get; set; }

        public int? DepartmentId { get; set; }

        public string DepartmentName { get; set; }

        public ActivityStatus Status { get; set; }

        public ProjectPriority Priority { get; set; }

        public DateTime? EstimatedStartDate { get; set; }

        public DateTime? EstimatedEndDate { get; set; }

        public DateTime? ActualStartDate { get; set; }

        public DateTime? ActualEndDate { get; set; }

        public decimal EstimatedHours { get; set; }

        public decimal WorkedHours { get; set; }

        public decimal ProgressPercentage { get; set; }

        public int? DaysElapsed { get; set; }

        public int? DaysRemaining { get; set; }

        public bool IsAtRisk { get; set; }
    }
}
