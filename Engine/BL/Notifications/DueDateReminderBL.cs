using Aelbry.BL.Email;
using Aelbry.DAL;

namespace Aelbry.BL.Notifications
{
    /// <summary>
    /// Recordatorios de vencimiento (Modulo 7): notificacion en la app + correo, una sola vez
    /// por actividad (ver ActivityDueReminder). El disparo periodico vive en
    /// DueDateReminderService (BackgroundService, Aelbry.Web); esta clase solo hace el trabajo.
    /// </summary>
    public class DueDateReminderBL
    {
        private readonly NotificationBL _notificationBL;
        private readonly IEmailSender _emailSender;

        public DueDateReminderBL(NotificationBL notificationBL, IEmailSender emailSender)
        {
            _notificationBL = notificationBL;
            _emailSender = emailSender;
        }

        public async Task<int> SendDueRemindersAsync(int daysAhead)
        {
            using (var dal = ActivityDueReminderDAL.Instance)
            {
                var candidates = dal.GetCandidates(daysAhead);

                foreach (var activity in candidates)
                {
                    string message = $"La actividad {activity.Code} - {activity.Name} vence el {activity.EstimatedEndDate:d}.";

                    _notificationBL.Create(
                        activity.ResponsibleUserId,
                        "Vencimiento proximo",
                        message,
                        $"/Activity/Index?projectId={activity.ProjectId}");

                    if (!string.IsNullOrWhiteSpace(activity.ResponsibleEmail))
                    {
                        try
                        {
                            await _emailSender.SendAsync(activity.ResponsibleEmail, "Vencimiento proximo - Aelbry", $"<p>{message}</p>");
                        }
                        catch (InvalidOperationException)
                        {
                            // SMTP no configurado: la notificacion en la app ya se genero, no se detiene el resto del lote.
                        }
                    }

                    dal.MarkSent(activity.ActivityId);
                }

                return candidates.Count;
            }
        }
    }
}
