using Aelbry.BO;
using Aelbry.DAL;

namespace Aelbry.BL
{
    /// <summary>
    /// Chat Empresarial (Modulo 6): persistencia de canales de proyecto y mensajes directos.
    /// No conoce nada de SignalR/grupos; el Hub decide a quien retransmitir en tiempo real
    /// usando el ProjectId/ConversationId que esta capa devuelve.
    /// </summary>
    public class ChatBL
    {
        public List<ChatConversation> GetConversations(int userId)
        {
            using (var dal = ChatConversationDAL.Instance)
            {
                return dal.GetByUser(userId);
            }
        }

        public int GetOrCreateConversation(int userId, int otherUserId)
        {
            if (userId == otherUserId)
            {
                throw new InvalidOperationException("No puedes iniciar una conversacion contigo mismo.");
            }

            using (var dal = ChatConversationDAL.Instance)
            {
                return dal.GetOrCreate(userId, otherUserId);
            }
        }

        public List<ChatMessage> GetProjectMessages(int projectId, int top = 50)
        {
            using (var dal = ChatMessageDAL.Instance)
            {
                var messages = dal.GetByProject(projectId, top);
                AttachReactions(messages, dal.GetReactionsByProject(projectId));
                return messages;
            }
        }

        public List<ChatMessage> GetDirectMessages(int conversationId, int top = 50)
        {
            using (var dal = ChatMessageDAL.Instance)
            {
                var messages = dal.GetByConversation(conversationId, top);
                AttachReactions(messages, dal.GetReactionsByConversation(conversationId));
                return messages;
            }
        }

        public ChatMessage SendProjectMessage(int projectId, int senderUserId, string text, int? parentMessageId)
        {
            using (var dal = ChatMessageDAL.Instance)
            {
                int id = dal.Insert(projectId, null, senderUserId, parentMessageId, text);
                return dal.GetById(id);
            }
        }

        public ChatMessage SendDirectMessage(int conversationId, int senderUserId, string text, int? parentMessageId)
        {
            using (var dal = ChatMessageDAL.Instance)
            {
                int id = dal.Insert(null, conversationId, senderUserId, parentMessageId, text);
                return dal.GetById(id);
            }
        }

        /// <summary>
        /// Devuelve el mensaje eliminado (con ProjectId/ConversationId) para que el Hub sepa
        /// a que grupo avisar del borrado.
        /// </summary>
        public ChatMessage DeleteMessage(int chatMessageId, int userId)
        {
            using (var dal = ChatMessageDAL.Instance)
            {
                dal.Delete(chatMessageId, userId);
                return dal.GetById(chatMessageId);
            }
        }

        public ChatReactionUpdateResult AddReaction(int chatMessageId, int userId, string emoji)
        {
            using (var dal = ChatMessageDAL.Instance)
            {
                var message = dal.GetById(chatMessageId)
                    ?? throw new InvalidOperationException("El mensaje no existe.");

                dal.AddReaction(chatMessageId, userId, emoji);

                return new ChatReactionUpdateResult
                {
                    ProjectId = message.ProjectId,
                    ConversationId = message.ConversationId,
                    Reactions = dal.GetReactionsByMessage(chatMessageId),
                };
            }
        }

        public ChatReactionUpdateResult RemoveReaction(int chatMessageId, int userId, string emoji)
        {
            using (var dal = ChatMessageDAL.Instance)
            {
                var message = dal.GetById(chatMessageId)
                    ?? throw new InvalidOperationException("El mensaje no existe.");

                dal.RemoveReaction(chatMessageId, userId, emoji);

                return new ChatReactionUpdateResult
                {
                    ProjectId = message.ProjectId,
                    ConversationId = message.ConversationId,
                    Reactions = dal.GetReactionsByMessage(chatMessageId),
                };
            }
        }

        private static void AttachReactions(List<ChatMessage> messages, List<ChatMessageReaction> allReactions)
        {
            var byMessage = allReactions.GroupBy(r => r.ChatMessageId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var message in messages)
            {
                message.Reactions = byMessage.TryGetValue(message.ChatMessageId, out var list) ? list : new List<ChatMessageReaction>();
            }
        }
    }
}
