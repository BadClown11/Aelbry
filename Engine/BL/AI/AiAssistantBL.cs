using Aelbry.BO;
using Aelbry.BO.AI;

namespace Aelbry.BL.AI
{
    /// <summary>
    /// Orquesta el minichat del asistente IA: pide la sugerencia a Gemini y, cuando el usuario
    /// elige que insertar, reutiliza ProjectBL/ActivityBL (mismo codigo, misma cascada de avance)
    /// en vez de escribir un camino de persistencia paralelo.
    /// </summary>
    public class AiAssistantBL
    {
        private readonly IActivitySuggestionService _suggestionService;
        private readonly ProjectBL _projectBL;
        private readonly ActivityBL _activityBL;

        public AiAssistantBL(IActivitySuggestionService suggestionService, ProjectBL projectBL, ActivityBL activityBL)
        {
            _suggestionService = suggestionService;
            _projectBL = projectBL;
            _activityBL = activityBL;
        }

        public Task<AiSuggestionResult> Suggest(string prompt)
        {
            return _suggestionService.SuggestAsync(prompt);
        }

        public InsertSuggestionsResult InsertSuggestions(InsertSuggestionsRequest request, int currentUserId)
        {
            int projectId = request.ProjectId ?? CreateProject(request, currentUserId);

            int insertedCount = 0;
            foreach (var phase in request.SelectedPhases.Where(p => p.Selected))
            {
                insertedCount += InsertTaskTree(phase, projectId, parentActivityId: null, currentUserId);
            }

            return new InsertSuggestionsResult { ProjectId = projectId, InsertedActivitiesCount = insertedCount };
        }

        private int CreateProject(InsertSuggestionsRequest request, int currentUserId)
        {
            var project = new Project
            {
                CompanyId = request.CompanyId,
                Code = request.NewProjectCode,
                Name = request.NewProjectName,
                ColorHex = "#4C6EF5",
                ProjectStatusId = request.NewProjectStatusId,
                Priority = ProjectPriority.Medium,
                RiskLevel = ProjectRiskLevel.Low,
                EstimatedHours = 0,
                ProjectManagerId = currentUserId,
                CreatedBy = currentUserId,
            };

            return _projectBL.Create(project);
        }

        private int InsertTaskTree(AiSuggestedTask task, int projectId, int? parentActivityId, int currentUserId)
        {
            if (!task.Selected)
            {
                return 0;
            }

            var activity = new Activity
            {
                ProjectId = projectId,
                ParentActivityId = parentActivityId,
                Name = task.Name,
                Description = task.Description,
                Category = task.SuggestedRole,
                ColorHex = "#4C6EF5",
                Status = ActivityStatus.Pending,
                Priority = ProjectPriority.Medium,
                ResponsibleUserId = currentUserId,
                Weight = 1,
                EstimatedHours = task.EstimatedHours,
                CreatedBy = currentUserId,
            };

            int newActivityId = _activityBL.Create(activity);
            int count = 1;

            foreach (var child in task.Subtasks)
            {
                count += InsertTaskTree(child, projectId, newActivityId, currentUserId);
            }

            return count;
        }
    }
}
