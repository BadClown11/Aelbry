using System.Security.Claims;
using Aelbry.BL;
using Aelbry.BL.Security;
using Aelbry.BO.Common;
using DataService.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Aelbry.Web.Controllers
{
    /// <summary>
    /// Centraliza el patron estandar: toda llamada a la capa BL se envuelve en try-catch y
    /// se devuelve siempre como JsonResult sobre el objeto Result (result = C.OK o ex.Message).
    /// </summary>
    public abstract class ApiControllerBase : Controller
    {
        protected JsonResult Exec<T>(Func<T> action)
        {
            var result = new Result();

            try
            {
                result.result = C.OK;
                result.data = action();
            }
            catch (Exception ex)
            {
                result.result = ex.Message;
            }

            return Json(result);
        }

        protected JsonResult Exec(Action action)
        {
            var result = new Result();

            try
            {
                action();
                result.result = C.OK;
            }
            catch (Exception ex)
            {
                result.result = ex.Message;
            }

            return Json(result);
        }

        /// <summary>
        /// Misma envoltura Result/try-catch que Exec, para las llamadas async a servicios
        /// externos (ej. la API de Gemini) que no pueden bloquearse con .Result/.Wait().
        /// </summary>
        protected async Task<JsonResult> ExecAsync<T>(Func<Task<T>> action)
        {
            var result = new Result();

            try
            {
                result.result = C.OK;
                result.data = await action();
            }
            catch (Exception ex)
            {
                result.result = ex.Message;
            }

            return Json(result);
        }

        protected string ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        protected int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        protected int CurrentCompanyId =>
            int.Parse(User.FindFirstValue(JwtTokenService.CompanyClaimType) ?? "0");

        protected string CurrentUserName =>
            $"{User.FindFirstValue(ClaimTypes.GivenName)} {User.FindFirstValue(ClaimTypes.Surname)}".Trim();

        /// <summary>
        /// Registra una accion sensible en la bitacora de auditoria (Modulo 9). Se llama
        /// explicitamente desde el subconjunto de acciones instrumentadas (ver Controllers de
        /// Usuarios/Roles/Empresas/Proyectos/Actividades), nunca automaticamente desde Exec,
        /// para mantener la cobertura deliberadamente parcial y explicita en cada sitio.
        /// </summary>
        protected void Audit(string module, string action, int? entityId = null, object dataBefore = null, object dataAfter = null)
        {
            var auditLogBL = HttpContext.RequestServices.GetRequiredService<AuditLogBL>();
            auditLogBL.Log(CurrentCompanyId, CurrentUserId, CurrentUserName, ClientIp, module, action, entityId, dataBefore, dataAfter);
        }

        protected static bool WasSuccessful(JsonResult jsonResult) =>
            jsonResult.Value is Result result && result.result == C.OK;
    }
}
