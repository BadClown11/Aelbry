using Aelbry.BO.AI;

namespace Aelbry.BL.AI
{
    /// <summary>
    /// Abstrae el proveedor de IA (Gemini) usado por el minichat del asistente para sugerir
    /// un arbol de fases/tareas/subtareas a partir de un prompt en lenguaje natural.
    /// </summary>
    public interface IActivitySuggestionService
    {
        Task<AiSuggestionResult> SuggestAsync(string prompt);
    }
}
