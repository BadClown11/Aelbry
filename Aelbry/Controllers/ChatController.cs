using Aelbry.BL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    /// <summary>
    /// Historial y listado de conversaciones via REST; el envio/recepcion en tiempo real,
    /// reacciones y presencia van por ChatHub (SignalR).
    /// </summary>
    [Authorize]
    public class ChatController : ApiControllerBase
    {
        private readonly ChatBL _chatBL;

        public ChatController(ChatBL chatBL)
        {
            _chatBL = chatBL;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Policy = "Permission:CHAT_USE")]
        public JsonResult GetConversations()
        {
            return Exec(() => _chatBL.GetConversations(CurrentUserId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:CHAT_USE")]
        public JsonResult GetOrCreateConversation(int otherUserId)
        {
            return Exec(() => _chatBL.GetOrCreateConversation(CurrentUserId, otherUserId));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:CHAT_USE")]
        public JsonResult GetProjectMessages(int projectId, int top = 50)
        {
            return Exec(() => _chatBL.GetProjectMessages(projectId, top));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:CHAT_USE")]
        public JsonResult GetDirectMessages(int conversationId, int top = 50)
        {
            return Exec(() => _chatBL.GetDirectMessages(conversationId, top));
        }
    }
}
