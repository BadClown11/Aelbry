namespace Aelbry.BO.Reports
{
    /// <summary>
    /// Filtros combinados del Modulo 8: por proyecto, empleado, equipo, departamento y rango
    /// de fecha de vencimiento (EstimatedEndDate). Todos opcionales salvo CompanyId.
    /// </summary>
    public class ReportFilter
    {
        public int CompanyId { get; set; }

        public int? ProjectId { get; set; }

        public int? UserId { get; set; }

        public int? TeamId { get; set; }

        public int? DepartmentId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
