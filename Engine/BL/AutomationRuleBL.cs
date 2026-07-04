using Aelbry.BO;
using Aelbry.DAL;

namespace Aelbry.BL
{
    /// <summary>
    /// CRUD administrativo de reglas de automatizacion. La ejecucion en tiempo real vive en
    /// AutomationEngineBL (esta clase no dispara nada, solo gestiona la configuracion).
    /// </summary>
    public class AutomationRuleBL
    {
        public List<AutomationRule> GetByCompany(int companyId)
        {
            using (var dal = AutomationRuleDAL.Instance)
            {
                return dal.GetByCompany(companyId);
            }
        }

        public AutomationRule GetById(int automationRuleId)
        {
            using (var dal = AutomationRuleDAL.Instance)
            {
                return dal.GetById(automationRuleId);
            }
        }

        public int Create(AutomationRule rule)
        {
            using (var dal = AutomationRuleDAL.Instance)
            {
                return dal.Create(rule);
            }
        }

        public void Update(AutomationRule rule)
        {
            using (var dal = AutomationRuleDAL.Instance)
            {
                dal.Update(rule);
            }
        }

        public void Delete(int automationRuleId, int modifiedBy)
        {
            using (var dal = AutomationRuleDAL.Instance)
            {
                dal.Delete(automationRuleId, modifiedBy);
            }
        }

        public List<AutomationRuleLog> GetLogs(int automationRuleId)
        {
            using (var dal = AutomationRuleDAL.Instance)
            {
                return dal.GetLogsByRule(automationRuleId);
            }
        }
    }
}
