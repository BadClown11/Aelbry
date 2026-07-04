namespace Aelbry.BO
{
    public class AutomationRuleLog
    {
        public int AutomationRuleLogId { get; set; }

        public int AutomationRuleId { get; set; }

        public DateTime TriggeredDate { get; set; }

        public bool Success { get; set; }

        public string Details { get; set; }
    }
}
