namespace Aelbry.BO
{
    /// <summary>
    /// Hilo de mensajes directos (DM) entre dos usuarios.
    /// </summary>
    public class ChatConversation
    {
        public int ConversationId { get; set; }

        public int OtherUserId { get; set; }

        public string OtherUserName { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
