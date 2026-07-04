using Aelbry.BL.Notifications;
using Aelbry.BO;
using Aelbry.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Aelbry.Web.Services
{
    /// <summary>
    /// Implementacion concreta (SignalR) de INotificationPublisher, definida aqui en el
    /// proyecto Web porque Engine no puede depender de Hubs/IHubContext (ver INotificationPublisher).
    /// </summary>
    public class SignalRNotificationPublisher : INotificationPublisher
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotificationPublisher(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task PublishAsync(Notification notification)
        {
            return _hubContext.Clients.Group(NotificationHub.UserGroup(notification.UserId)).SendAsync("ReceiveNotification", notification);
        }
    }
}
