using Aelbry.BL.AI;
using Aelbry.BO.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    [Authorize]
    public class AiAssistantController : ApiControllerBase
    {
        private readonly AiAssistantBL _aiAssistantBL;

        public AiAssistantController(AiAssistantBL aiAssistantBL)
        {
            _aiAssistantBL = aiAssistantBL;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Policy = "Permission:AI_ASSISTANT_USE")]
        public Task<JsonResult> Suggest([FromBody] AiSuggestionRequest request)
        {
            return ExecAsync(() => _aiAssistantBL.Suggest(request.Prompt));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:AI_ASSISTANT_USE")]
        public JsonResult InsertSuggestions([FromBody] InsertSuggestionsRequest request)
        {
            return Exec(() => _aiAssistantBL.InsertSuggestions(request, CurrentUserId));
        }
    }
}
