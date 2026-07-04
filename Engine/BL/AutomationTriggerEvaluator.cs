namespace Aelbry.BL
{
    /// <summary>
    /// Logica pura (sin acceso a datos) para decidir si un disparador de umbral de avance
    /// (Modulo 7) debe activarse: solo cuando el valor cruza el umbral hacia arriba en esta
    /// actualizacion puntual, para no volver a disparar la regla en cada recalculo posterior.
    /// </summary>
    public static class AutomationTriggerEvaluator
    {
        public static bool CrossesThresholdUpward(decimal previousValue, decimal newValue, decimal threshold)
        {
            return previousValue < threshold && newValue >= threshold;
        }
    }
}
