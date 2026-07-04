namespace Aelbry.BO
{
    /// <summary>
    /// Actividad proxima a vencer que aun no genero su recordatorio (ver ActivityDueReminderDAL
    /// y el BackgroundService DueDateReminderService en Aelbry.Web).
    /// </summary>
    public class ActivityDueReminderCandidate
    {
        public int ActivityId { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public DateTime EstimatedEndDate { get; set; }

        public int ResponsibleUserId { get; set; }

        public string ResponsibleEmail { get; set; }

        public string ResponsibleName { get; set; }

        public int ProjectId { get; set; }
    }
}
