namespace Aelbry.BL
{
    /// <summary>
    /// Logica pura (sin acceso a datos) del algoritmo de avance ponderado del Modulo 3.
    /// El BL es responsable de cargar los datos, invocar este calculo, y persistir el resultado.
    /// </summary>
    public static class ActivityProgressCalculator
    {
        /// <summary>
        /// Avance de una actividad hoja (sin subactividades): prioriza el checklist si existe,
        /// y cae a horas trabajadas/estimadas si no hay checklist.
        /// </summary>
        public static decimal CalculateLeafProgress(int checkedItemsCount, int totalItemsCount, decimal workedHours, decimal estimatedHours)
        {
            if (totalItemsCount > 0)
            {
                return Math.Round((decimal)checkedItemsCount / totalItemsCount * 100m, 2);
            }

            if (estimatedHours > 0)
            {
                return Math.Min(100m, Math.Round(workedHours / estimatedHours * 100m, 2));
            }

            return 0m;
        }

        /// <summary>
        /// Avance de una actividad (o del proyecto) con subactividades: promedio ponderado
        /// por el campo Weight de cada hijo.
        /// </summary>
        public static decimal CalculateWeightedProgress(IEnumerable<(decimal Weight, decimal Progress)> children)
        {
            decimal totalWeight = 0m;
            decimal weightedSum = 0m;

            foreach (var child in children)
            {
                totalWeight += child.Weight;
                weightedSum += child.Weight * child.Progress;
            }

            if (totalWeight <= 0m)
            {
                return 0m;
            }

            return Math.Round(weightedSum / totalWeight, 2);
        }
    }
}
