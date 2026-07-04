namespace Aelbry.BL
{
    /// <summary>
    /// Logica pura del KPI de productividad semanal (Modulo 8).
    /// </summary>
    public static class ReportKpiCalculator
    {
        public static decimal CalculateProductivityPercent(int completed, int due)
        {
            if (due <= 0)
            {
                return 0m;
            }

            return Math.Round(completed * 100m / due, 2);
        }
    }
}
