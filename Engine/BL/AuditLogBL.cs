using System.Text.Json;
using Aelbry.BO;
using Aelbry.DAL;

namespace Aelbry.BL
{
    /// <summary>
    /// Bitacora de auditoria (Modulo 9). Se invoca explicitamente desde un subconjunto
    /// representativo de acciones sensibles (ver ApiControllerBase.Audit) en vez de
    /// instrumentar cada metodo de cada BL; es una cobertura deliberadamente parcial.
    /// </summary>
    public class AuditLogBL
    {
        public void Log(int companyId, int userId, string userName, string ipAddress, string module, string action, int? entityId, object dataBefore, object dataAfter)
        {
            string before = dataBefore != null ? JsonSerializer.Serialize(dataBefore) : null;
            string after = dataAfter != null ? JsonSerializer.Serialize(dataAfter) : null;

            using (var dal = AuditLogDAL.Instance)
            {
                dal.Insert(companyId, userId, userName, ipAddress, module, action, entityId, before, after);
            }
        }

        public List<AuditLog> GetByCompany(AuditLogFilter filter)
        {
            using (var dal = AuditLogDAL.Instance)
            {
                return dal.GetByCompany(filter);
            }
        }
    }
}
