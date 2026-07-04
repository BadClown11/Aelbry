namespace Aelbry.BO.AI
{
    /// <summary>
    /// Arbol completo devuelto por Gemini: nombre/descripcion de proyecto sugeridos y las
    /// fases (cada una con tareas y subtareas) listas para mostrarse en el minichat.
    /// </summary>
    public class AiSuggestionResult
    {
        public string SuggestedProjectName { get; set; }

        public string SuggestedProjectDescription { get; set; }

        public List<AiSuggestedTask> Phases { get; set; } = new List<AiSuggestedTask>();
    }
}
