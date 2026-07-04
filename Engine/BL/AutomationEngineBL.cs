using Aelbry.BL.Notifications;
using Aelbry.BO;
using Aelbry.DAL;

namespace Aelbry.BL
{
    /// <summary>
    /// Ejecucion en tiempo real de las reglas de automatizacion (Modulo 7). Deliberadamente usa
    /// las DAL directamente (no ActivityBL/ProjectBL) para evitar una dependencia circular, ya
    /// que ActivityBL es quien invoca a este motor tras cada cambio de estado/avance. Como
    /// consecuencia, las acciones de una regla NO vuelven a disparar otras reglas en cascada
    /// (evita bucles infinitos); es una limitacion conocida de esta primera version.
    /// NotificationBL si puede inyectarse sin ciclo, porque no depende de ActivityBL/ProjectBL.
    /// </summary>
    public class AutomationEngineBL
    {
        private readonly NotificationBL _notificationBL;

        public AutomationEngineBL(NotificationBL notificationBL)
        {
            _notificationBL = notificationBL;
        }

        public void CheckActivityStatusChanged(int activityId, ActivityStatus newStatus, int triggeredBy)
        {
            using (var ruleDal = AutomationRuleDAL.Instance)
            {
                var rules = ruleDal.GetActiveByActivityTrigger(activityId, AutomationTriggerType.ActivityStatusChanged)
                    .Where(r => r.TriggerStatus == newStatus);

                foreach (var rule in rules)
                {
                    ExecuteRule(ruleDal, rule, triggeredBy);
                }
            }
        }

        public void CheckActivityProgressThreshold(int activityId, decimal previousProgress, decimal newProgress, int triggeredBy)
        {
            using (var ruleDal = AutomationRuleDAL.Instance)
            {
                var rules = ruleDal.GetActiveByActivityTrigger(activityId, AutomationTriggerType.ActivityProgressThreshold)
                    .Where(r => r.TriggerThresholdPercent.HasValue
                        && AutomationTriggerEvaluator.CrossesThresholdUpward(previousProgress, newProgress, r.TriggerThresholdPercent.Value));

                foreach (var rule in rules)
                {
                    ExecuteRule(ruleDal, rule, triggeredBy);
                }
            }
        }

        public void CheckProjectProgressThreshold(int projectId, decimal previousProgress, decimal newProgress, int triggeredBy)
        {
            using (var ruleDal = AutomationRuleDAL.Instance)
            {
                var rules = ruleDal.GetActiveByProjectTrigger(projectId, AutomationTriggerType.ProjectProgressThreshold)
                    .Where(r => r.TriggerThresholdPercent.HasValue
                        && AutomationTriggerEvaluator.CrossesThresholdUpward(previousProgress, newProgress, r.TriggerThresholdPercent.Value));

                foreach (var rule in rules)
                {
                    ExecuteRule(ruleDal, rule, triggeredBy);
                }
            }
        }

        private void ExecuteRule(AutomationRuleDAL ruleDal, AutomationRule rule, int triggeredBy)
        {
            try
            {
                switch (rule.ActionType)
                {
                    case AutomationActionType.ChangeActivityStatus:
                        using (var activityDal = ActivityDAL.Instance)
                        {
                            activityDal.UpdateStatus(rule.ActionTargetActivityId!.Value, rule.ActionNewActivityStatus!.Value, triggeredBy);
                        }
                        break;

                    case AutomationActionType.ChangeProjectStatus:
                        using (var projectDal = ProjectDAL.Instance)
                        {
                            projectDal.UpdateStatus(rule.ActionTargetProjectId!.Value, rule.ActionNewProjectStatusId!.Value, triggeredBy);
                        }
                        break;

                    case AutomationActionType.SendNotification:
                        _notificationBL.Create(
                            rule.ActionNotificationUserId!.Value,
                            $"Automatizacion: {rule.Name}",
                            rule.ActionNotificationMessage ?? rule.Name,
                            link: null);
                        break;
                }

                ruleDal.AddLog(rule.AutomationRuleId, success: true, $"Regla '{rule.Name}' ejecutada: {rule.ActionType}.");
            }
            catch (Exception ex)
            {
                ruleDal.AddLog(rule.AutomationRuleId, success: false, ex.Message);
            }
        }
    }
}
