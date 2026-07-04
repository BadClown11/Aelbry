namespace Aelbry.BO.Reports
{
    /// <summary>
    /// Resumen ejecutivo (Modulo 8): totales por estado (snapshot actual, sin filtro de fecha)
    /// y productividad semanal comparada contra la semana anterior.
    /// Productividad = (actividades completadas cuya fecha real de fin cae en la semana) /
    /// (actividades cuya fecha estimada de fin cae en la semana) * 100. No esta acotada a 100
    /// porque puede completarse trabajo que estaba programado para otra semana.
    /// </summary>
    public class KpiSummary
    {
        public Dictionary<ActivityStatus, int> CountsByStatus { get; set; } = new Dictionary<ActivityStatus, int>();

        public int DueThisWeek { get; set; }

        public int CompletedThisWeek { get; set; }

        public decimal ProductivityThisWeek { get; set; }

        public int DueLastWeek { get; set; }

        public int CompletedLastWeek { get; set; }

        public decimal ProductivityLastWeek { get; set; }

        public decimal ProductivityDelta { get; set; }
    }
}
