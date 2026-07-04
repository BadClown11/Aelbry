namespace Aelbry.BO
{
    /// <summary>
    /// Mensaje de un canal de proyecto (ProjectId) o de una conversacion directa (ConversationId);
    /// exactamente uno de los dos esta presente. ParentMessageId habilita hilos de un solo nivel.
    /// </summary>
    public class ChatMessage
    {
        public int ChatMessageId { get; set; }

        public int? ProjectId { get; set; }

        public int? ConversationId { get; set; }

        public int SenderUserId { get; set; }

        public string SenderName { get; set; }

        public int? ParentMessageId { get; set; }

        public string Text { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool IsDeleted { get; set; }

        public List<ChatMessageReaction> Reactions { get; set; } = new List<ChatMessageReaction>();
    }
}
