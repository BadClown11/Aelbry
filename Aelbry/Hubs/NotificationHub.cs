using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Aelbry.Web.Hubs
{
    /// <summary>
    /// Push de notificaciones en tiempo real (Modulo 7). A diferencia de ChatHub (que solo se
    /// conecta dentro de la pagina de Chat), este Hub se conecta desde cualquier pagina
    /// (ver common.js) para que la campanita del navbar reciba avisos en toda la aplicacion.
    /// </summary>
    [Authorize(Policy = "Permission:NOTIFICATIONS_USE")]
    public class NotificationHub : Hub
    {
        public static string UserGroup(int userId) => $"user-{userId}";

        public override async Task OnConnectedAsync()
        {
            int userId = int.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
            await base.OnConnectedAsync();
        }
    }
}
