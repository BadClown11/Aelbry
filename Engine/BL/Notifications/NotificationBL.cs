using Aelbry.BO;
using Aelbry.DAL;

namespace Aelbry.BL.Notifications
{
    /// <summary>
    /// Persiste notificaciones y las publica en tiempo real via INotificationPublisher
    /// (implementado con SignalR en Aelbry.Web).
    /// </summary>
    public class NotificationBL
    {
        private readonly INotificationPublisher _publisher;

        public NotificationBL(INotificationPublisher publisher)
        {
            _publisher = publisher;
        }

        public List<Notification> GetByUser(int userId, bool unreadOnly)
        {
            using (var dal = NotificationDAL.Instance)
            {
                return dal.GetByUser(userId, unreadOnly);
            }
        }

        public int GetUnreadCount(int userId)
        {
            using (var dal = NotificationDAL.Instance)
            {
                return dal.GetUnreadCount(userId);
            }
        }

        public Notification Create(int userId, string title, string message, string link)
        {
            Notification notification;

            using (var dal = NotificationDAL.Instance)
            {
                int id = dal.Create(userId, title, message, link);
                notification = dal.GetById(id);
            }

            // Create() es sincrono porque lo invocan flujos sincronos existentes (AutomationEngineBL).
            // Bloquear aqui es seguro: ASP.NET Core (Kestrel) no tiene SynchronizationContext, asi que
            // no hay riesgo de deadlock como lo habria en ASP.NET Framework clasico.
            _publisher.PublishAsync(notification).GetAwaiter().GetResult();

            return notification;
        }

        public void MarkAsRead(int notificationId, int userId)
        {
            using (var dal = NotificationDAL.Instance)
            {
                dal.MarkAsRead(notificationId, userId);
            }
        }

        public void MarkAllAsRead(int userId)
        {
            using (var dal = NotificationDAL.Instance)
            {
                dal.MarkAllAsRead(userId);
            }
        }
    }
}
