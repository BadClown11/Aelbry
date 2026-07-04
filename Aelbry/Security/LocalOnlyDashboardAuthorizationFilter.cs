using System.Net;
using Hangfire.Dashboard;

namespace Aelbry.Web.Security
{
    /// <summary>
    /// La app solo tiene autenticacion JWT por header (sin cookies/sesion de navegador), asi
    /// que un usuario con sesion de Admin en el SPA no queda "autenticado" cuando el navegador
    /// navega directamente a /hangfire. En vez de montar un segundo esquema de auth solo para
    /// este panel, se restringe el Dashboard a conexiones locales (loopback); para acceso
    /// remoto real, ponerlo detras de un tunel/VPN o de un reverse proxy con su propio login.
    /// </summary>
    public class LocalOnlyDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            var remoteIp = httpContext.Connection.RemoteIpAddress;
            return remoteIp != null && IPAddress.IsLoopback(remoteIp);
        }
    }
}
