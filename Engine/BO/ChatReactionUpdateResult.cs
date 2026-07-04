namespace Aelbry.BO
{
    /// <summary>
    /// Resultado de agregar/quitar una reaccion: identifica el canal (proyecto o conversacion)
    /// al que pertenece el mensaje, para que el Hub de SignalR sepa a que grupo retransmitir,
    /// sin que la capa BL conozca nada de SignalR ni de grupos.
    /// </summary>
    public class ChatReactionUpdateResult
    {
        public int? ProjectId { get; set; }

        public int? ConversationId { get; set; }

        public List<ChatMessageReaction> Reactions { get; set; } = new List<ChatMessageReaction>();
    }
}
