namespace Aelbry.BO
{
    /// <summary>
    /// Resumen de un usuario asignado como participante/colaborador de una actividad
    /// (el responsable principal vive en Activity.ResponsibleUserId).
    /// </summary>
    public class ActivityParticipant
    {
        public int UserId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string JobTitle { get; set; }

        public string PhotoUrl { get; set; }

        public DateTime AddedDate { get; set; }
    }
}
