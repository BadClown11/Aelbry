namespace Aelbry.BO
{
    /// <summary>
    /// Resultado del calculo de camino critico (CPM) para una actividad, expresado en dias
    /// relativos al inicio del proyecto (no son fechas absolutas).
    /// </summary>
    public class ActivityCriticalPathInfo
    {
        public int ActivityId { get; set; }

        public decimal EarlyStart { get; set; }

        public decimal EarlyFinish { get; set; }

        public decimal LateStart { get; set; }

        public decimal LateFinish { get; set; }

        public decimal Slack { get; set; }

        public bool IsCritical { get; set; }
    }
}
