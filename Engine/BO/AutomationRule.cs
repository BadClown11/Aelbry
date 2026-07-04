using Aelbry.BO.Common;

namespace Aelbry.BO
{
    /// <summary>
    /// Regla de automatizacion (Modulo 7): "si [Trigger] entonces [Action]". Las columnas de
    /// disparador/accion no usadas por el tipo elegido quedan en null (ver AutomationEngineBL).
    /// </summary>
    public class AutomationRule : AuditableEntity
    {
        public int AutomationRuleId { get; set; }

        public int CompanyId { get; set; }

        public string Name { get; set; }

        public AutomationTriggerType TriggerType { get; set; }

        public int? TriggerActivityId { get; set; }

        public string TriggerActivityName { get; set; }

        public int? TriggerProjectId { get; set; }

        public string TriggerProjectName { get; set; }

        public decimal? TriggerThresholdPercent { get; set; }

        public ActivityStatus? TriggerStatus { get; set; }

        public AutomationActionType ActionType { get; set; }

        public int? ActionTargetActivityId { get; set; }

        public string ActionTargetActivityName { get; set; }

        public int? ActionTargetProjectId { get; set; }

        public string ActionTargetProjectName { get; set; }

        public ActivityStatus? ActionNewActivityStatus { get; set; }

        public int? ActionNewProjectStatusId { get; set; }

        public string ActionNotificationMessage { get; set; }

        public int? ActionNotificationUserId { get; set; }

        public string ActionNotificationUserName { get; set; }

        public bool IsActive { get; set; }
    }
}
