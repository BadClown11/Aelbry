namespace Aelbry.BO
{
    /// <summary>
    /// Tipo de bloqueo entre una actividad y su predecesora (DependsOnActivityId).
    /// </summary>
    public enum DependencyType
    {
        FinishToStart = 1,
        StartToStart = 2,
        FinishToFinish = 3,
        StartToFinish = 4,
    }
}
