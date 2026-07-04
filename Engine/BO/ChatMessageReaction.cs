namespace Aelbry.BO
{
    public class ChatMessageReaction
    {
        public int ChatMessageId { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }

        public string Emoji { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
