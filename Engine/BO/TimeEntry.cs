using Aelbry.BO.Common;

namespace Aelbry.BO
{
    public class TimeEntry : AuditableEntity
    {
        public int TimeEntryId { get; set; }

        public int ActivityId { get; set; }

        public string ActivityName { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public decimal DurationHours { get; set; }

        public bool IsManual { get; set; }

        public bool IsOvertime { get; set; }

        public string Notes { get; set; }
    }
}
