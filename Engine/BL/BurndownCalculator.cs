using Aelbry.BO.Reports;

namespace Aelbry.BL
{
    /// <summary>
    /// Logica pura del burndown (Modulo 8): linea ideal (lineal desde el total de horas
    /// estimadas hasta 0) contra la linea real (horas de actividades aun no completadas a
    /// cada dia, segun ActualEndDate). No requiere historial: se reconstruye desde el estado
    /// actual de las actividades.
    /// </summary>
    public static class BurndownCalculator
    {
        public class ActivitySnapshot
        {
            public decimal EstimatedHours { get; set; }

            public bool IsCompleted { get; set; }

            public DateTime? ActualEndDate { get; set; }
        }

        public static List<BurndownPoint> Calculate(List<ActivitySnapshot> activities, DateTime startDate, DateTime endDate)
        {
            var points = new List<BurndownPoint>();

            decimal totalHours = activities.Sum(a => a.EstimatedHours);
            int totalDays = Math.Max(1, (endDate.Date - startDate.Date).Days);

            for (var day = startDate.Date; day <= endDate.Date; day = day.AddDays(1))
            {
                int dayIndex = (day - startDate.Date).Days;
                decimal ideal = Math.Max(0m, totalHours - (totalHours * dayIndex / totalDays));

                decimal actualRemaining = activities
                    .Where(a => !(a.IsCompleted && a.ActualEndDate.HasValue && a.ActualEndDate.Value.Date <= day))
                    .Sum(a => a.EstimatedHours);

                points.Add(new BurndownPoint
                {
                    Date = day,
                    IdealRemainingHours = ideal,
                    ActualRemainingHours = actualRemaining,
                });
            }

            return points;
        }
    }
}
