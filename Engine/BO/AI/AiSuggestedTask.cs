namespace Aelbry.BO.AI
{
    /// <summary>
    /// Nodo del arbol sugerido por el asistente IA (fase, tarea o subtarea). No es una entidad
    /// persistida: solo existe en memoria hasta que el usuario elige insertarlo como Activity.
    /// </summary>
    public class AiSuggestedTask
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public decimal EstimatedHours { get; set; }

        public string SuggestedRole { get; set; }

        public bool Selected { get; set; } = true;

        public List<AiSuggestedTask> Subtasks { get; set; } = new List<AiSuggestedTask>();
    }
}
