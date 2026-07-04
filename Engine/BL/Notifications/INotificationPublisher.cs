using Aelbry.BO;

namespace Aelbry.BL.Notifications
{
    /// <summary>
    /// Abstrae la entrega en tiempo real de una notificacion. Se define aqui (Engine) para que
    /// NotificationBL/AutomationEngineBL no dependan de SignalR ni del proyecto Web; la
    /// implementacion concreta (SignalRNotificationPublisher) vive en Aelbry.Web.
    /// </summary>
    public interface INotificationPublisher
    {
        Task PublishAsync(Notification notification);
    }
}
