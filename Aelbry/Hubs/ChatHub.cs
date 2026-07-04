using System.Collections.Concurrent;
using System.Security.Claims;
using Aelbry.BL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Aelbry.Web.Hubs
{
    /// <summary>
    /// Chat Empresarial en tiempo real (Modulo 6). Los canales de proyecto y las conversaciones
    /// directas se persisten via ChatBL; este Hub solo orquesta grupos de SignalR, presencia
    /// (online/offline por conteo de conexiones) y el evento de "escribiendo...".
    /// </summary>
    [Authorize(Policy = "Permission:CHAT_USE")]
    public class ChatHub : Hub
    {
        // Conteo de conexiones activas por usuario (una persona puede tener varias pestanas abiertas).
        private static readonly ConcurrentDictionary<int, int> OnlineUserConnectionCounts = new();

        private readonly ChatBL _chatBL;

        public ChatHub(ChatBL chatBL)
        {
            _chatBL = chatBL;
        }

        public static string ProjectGroup(int projectId) => $"project-{projectId}";

        public static string ConversationGroup(int conversationId) => $"conversation-{conversationId}";

        private int CurrentUserId => int.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        public override async Task OnConnectedAsync()
        {
            int userId = CurrentUserId;
            int count = OnlineUserConnectionCounts.AddOrUpdate(userId, 1, (_, existing) => existing + 1);

            if (count == 1)
            {
                await Clients.All.SendAsync("UserOnline", userId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            int userId = CurrentUserId;
            int count = OnlineUserConnectionCounts.AddOrUpdate(userId, 0, (_, existing) => Math.Max(0, existing - 1));

            if (count == 0)
            {
                OnlineUserConnectionCounts.TryRemove(userId, out _);
                await Clients.All.SendAsync("UserOffline", userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinProjectChannel(int projectId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ProjectGroup(projectId));
        }

        public async Task LeaveProjectChannel(int projectId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, ProjectGroup(projectId));
        }

        public async Task JoinConversation(int conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ConversationGroup(conversationId));
        }

        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, ConversationGroup(conversationId));
        }

        public async Task SendProjectMessage(int projectId, string text, int? parentMessageId)
        {
            var message = _chatBL.SendProjectMessage(projectId, CurrentUserId, text, parentMessageId);
            await Clients.Group(ProjectGroup(projectId)).SendAsync("ReceiveMessage", message);
        }

        public async Task SendDirectMessage(int conversationId, string text, int? parentMessageId)
        {
            var message = _chatBL.SendDirectMessage(conversationId, CurrentUserId, text, parentMessageId);
            await Clients.Group(ConversationGroup(conversationId)).SendAsync("ReceiveMessage", message);
        }

        public async Task DeleteMessage(int chatMessageId)
        {
            var message = _chatBL.DeleteMessage(chatMessageId, CurrentUserId);
            string groupName = message.ProjectId.HasValue ? ProjectGroup(message.ProjectId.Value) : ConversationGroup(message.ConversationId!.Value);
            await Clients.Group(groupName).SendAsync("MessageDeleted", chatMessageId);
        }

        public async Task AddReaction(int chatMessageId, string emoji)
        {
            var result = _chatBL.AddReaction(chatMessageId, CurrentUserId, emoji);
            await BroadcastReactions(chatMessageId, result);
        }

        public async Task RemoveReaction(int chatMessageId, string emoji)
        {
            var result = _chatBL.RemoveReaction(chatMessageId, CurrentUserId, emoji);
            await BroadcastReactions(chatMessageId, result);
        }

        private async Task BroadcastReactions(int chatMessageId, BO.ChatReactionUpdateResult result)
        {
            string groupName = result.ProjectId.HasValue ? ProjectGroup(result.ProjectId.Value) : ConversationGroup(result.ConversationId!.Value);
            await Clients.Group(groupName).SendAsync("ReactionsUpdated", chatMessageId, result.Reactions);
        }

        public async Task NotifyTypingInProject(int projectId, bool isTyping)
        {
            await Clients.OthersInGroup(ProjectGroup(projectId)).SendAsync("UserTyping", CurrentUserId, isTyping);
        }

        public async Task NotifyTypingInConversation(int conversationId, bool isTyping)
        {
            await Clients.OthersInGroup(ConversationGroup(conversationId)).SendAsync("UserTyping", CurrentUserId, isTyping);
        }
    }
}
