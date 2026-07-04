namespace Aelbry.BO.AI
{
    /// <summary>
    /// Lo que el usuario decidio insertar tras revisar la sugerencia del asistente IA en el
    /// minichat (checkboxes desmarcados no llegan en SelectedPhases).
    /// </summary>
    public class InsertSuggestionsRequest
    {
        public int? ProjectId { get; set; }

        public int CompanyId { get; set; }

        public string NewProjectCode { get; set; }

        public string NewProjectName { get; set; }

        public int NewProjectStatusId { get; set; }

        public List<AiSuggestedTask> SelectedPhases { get; set; } = new List<AiSuggestedTask>();
    }
}
