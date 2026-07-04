using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Aelbry.BO.AI;
using Microsoft.Extensions.Options;

namespace Aelbry.BL.AI
{
    /// <summary>
    /// Llama a la API de Gemini (generateContent con responseSchema en JSON estricto) para
    /// convertir un prompt en lenguaje natural en un arbol de fases/tareas/subtareas.
    /// </summary>
    public class GeminiActivitySuggestionService : IActivitySuggestionService
    {
        private static readonly JsonSerializerOptions DeserializeOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        private readonly HttpClient _httpClient;
        private readonly GeminiOptions _options;

        public GeminiActivitySuggestionService(HttpClient httpClient, IOptions<GeminiOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<AiSuggestionResult> SuggestAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                throw new InvalidOperationException(
                    "La API key de Gemini no esta configurada. Agrega el valor de Gemini:ApiKey en appsettings o user-secrets.");
            }

            string url = $"{_options.BaseUrl.TrimEnd('/')}/{_options.Model}:generateContent?key={Uri.EscapeDataString(_options.ApiKey)}";

            var request = new GeminiRequest
            {
                Contents = new List<GeminiContent>
                {
                    new GeminiContent { Parts = new List<GeminiPart> { new GeminiPart { Text = BuildPrompt(prompt) } } },
                },
                GenerationConfig = new GeminiGenerationConfig { ResponseSchema = BuildResponseSchema() },
            };

            using var httpResponse = await _httpClient.PostAsJsonAsync(url, request);
            httpResponse.EnsureSuccessStatusCode();

            var geminiResponse = await httpResponse.Content.ReadFromJsonAsync<GeminiResponse>();
            string json = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidOperationException("Gemini no devolvio contenido valido.");
            }

            var dto = JsonSerializer.Deserialize<GeminiSuggestionDto>(json, DeserializeOptions)
                ?? throw new InvalidOperationException("No se pudo interpretar la respuesta de Gemini.");

            return MapToResult(dto);
        }

        private static string BuildPrompt(string userPrompt)
        {
            return "Eres un asistente de gestion de proyectos empresariales. A partir de la siguiente peticion del " +
                   "usuario, genera un plan de proyecto dividido en fases, cada fase con tareas, y cada tarea con " +
                   "subtareas cuando tenga sentido. Incluye horas estimadas realistas y el rol sugerido (ej. " +
                   "Desarrollador Backend, Disenador UX, QA) para cada elemento. Responde en espanol. " +
                   $"Peticion del usuario: {userPrompt}";
        }

        private static object BuildResponseSchema()
        {
            var subtaskSchema = new
            {
                type = "OBJECT",
                properties = new
                {
                    name = new { type = "STRING" },
                    description = new { type = "STRING" },
                    estimatedHours = new { type = "NUMBER" },
                    suggestedRole = new { type = "STRING" },
                },
                required = new[] { "name" },
            };

            var taskSchema = new
            {
                type = "OBJECT",
                properties = new
                {
                    name = new { type = "STRING" },
                    description = new { type = "STRING" },
                    estimatedHours = new { type = "NUMBER" },
                    suggestedRole = new { type = "STRING" },
                    subtasks = new { type = "ARRAY", items = subtaskSchema },
                },
                required = new[] { "name" },
            };

            var phaseSchema = new
            {
                type = "OBJECT",
                properties = new
                {
                    name = new { type = "STRING" },
                    description = new { type = "STRING" },
                    estimatedHours = new { type = "NUMBER" },
                    suggestedRole = new { type = "STRING" },
                    tasks = new { type = "ARRAY", items = taskSchema },
                },
                required = new[] { "name" },
            };

            return new
            {
                type = "OBJECT",
                properties = new
                {
                    projectName = new { type = "STRING" },
                    projectDescription = new { type = "STRING" },
                    phases = new { type = "ARRAY", items = phaseSchema },
                },
                required = new[] { "projectName", "phases" },
            };
        }

        private static AiSuggestionResult MapToResult(GeminiSuggestionDto dto)
        {
            return new AiSuggestionResult
            {
                SuggestedProjectName = dto.ProjectName,
                SuggestedProjectDescription = dto.ProjectDescription,
                Phases = (dto.Phases ?? new List<GeminiPhaseDto>()).Select(MapPhase).ToList(),
            };
        }

        private static AiSuggestedTask MapPhase(GeminiPhaseDto phase)
        {
            return new AiSuggestedTask
            {
                Name = phase.Name,
                Description = phase.Description,
                EstimatedHours = phase.EstimatedHours,
                SuggestedRole = phase.SuggestedRole,
                Subtasks = (phase.Tasks ?? new List<GeminiTaskDto>()).Select(MapTask).ToList(),
            };
        }

        private static AiSuggestedTask MapTask(GeminiTaskDto task)
        {
            return new AiSuggestedTask
            {
                Name = task.Name,
                Description = task.Description,
                EstimatedHours = task.EstimatedHours,
                SuggestedRole = task.SuggestedRole,
                Subtasks = (task.Subtasks ?? new List<GeminiSubtaskDto>()).Select(MapSubtask).ToList(),
            };
        }

        private static AiSuggestedTask MapSubtask(GeminiSubtaskDto subtask)
        {
            return new AiSuggestedTask
            {
                Name = subtask.Name,
                Description = subtask.Description,
                EstimatedHours = subtask.EstimatedHours,
                SuggestedRole = subtask.SuggestedRole,
            };
        }

        // ---- Formas de intercambio (wire format) especificas de la API de Gemini ----

        private class GeminiRequest
        {
            [JsonPropertyName("contents")]
            public List<GeminiContent> Contents { get; set; } = new();

            [JsonPropertyName("generationConfig")]
            public GeminiGenerationConfig GenerationConfig { get; set; }
        }

        private class GeminiGenerationConfig
        {
            [JsonPropertyName("responseMimeType")]
            public string ResponseMimeType { get; set; } = "application/json";

            [JsonPropertyName("responseSchema")]
            public object ResponseSchema { get; set; }
        }

        private class GeminiContent
        {
            [JsonPropertyName("parts")]
            public List<GeminiPart> Parts { get; set; } = new();
        }

        private class GeminiPart
        {
            [JsonPropertyName("text")]
            public string Text { get; set; }
        }

        private class GeminiResponse
        {
            [JsonPropertyName("candidates")]
            public List<GeminiCandidate> Candidates { get; set; } = new();
        }

        private class GeminiCandidate
        {
            [JsonPropertyName("content")]
            public GeminiContent Content { get; set; }
        }

        private class GeminiSuggestionDto
        {
            public string ProjectName { get; set; }

            public string ProjectDescription { get; set; }

            public List<GeminiPhaseDto> Phases { get; set; } = new();
        }

        private class GeminiPhaseDto
        {
            public string Name { get; set; }

            public string Description { get; set; }

            public decimal EstimatedHours { get; set; }

            public string SuggestedRole { get; set; }

            public List<GeminiTaskDto> Tasks { get; set; } = new();
        }

        private class GeminiTaskDto
        {
            public string Name { get; set; }

            public string Description { get; set; }

            public decimal EstimatedHours { get; set; }

            public string SuggestedRole { get; set; }

            public List<GeminiSubtaskDto> Subtasks { get; set; } = new();
        }

        private class GeminiSubtaskDto
        {
            public string Name { get; set; }

            public string Description { get; set; }

            public decimal EstimatedHours { get; set; }

            public string SuggestedRole { get; set; }
        }
    }
}
