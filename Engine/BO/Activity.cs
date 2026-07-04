using Aelbry.BO.Common;

namespace Aelbry.BO
{
    public class Activity : AuditableEntity
    {
        public int ActivityId { get; set; }

        public int ProjectId { get; set; }

        public int? ParentActivityId { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Category { get; set; }

        public string ColorHex { get; set; }

        public ActivityStatus Status { get; set; }

        public ProjectPriority Priority { get; set; }

        public int ResponsibleUserId { get; set; }

        public string ResponsibleName { get; set; }

        public DateTime? EstimatedStartDate { get; set; }

        public DateTime? EstimatedEndDate { get; set; }

        public DateTime? ActualStartDate { get; set; }

        public DateTime? ActualEndDate { get; set; }

        public decimal Weight { get; set; }

        public decimal EstimatedHours { get; set; }

        public decimal WorkedHours { get; set; }

        public decimal ProgressPercentage { get; set; }

        public int Sequence { get; set; }

        public bool IsActive { get; set; }

        public List<Activity> Children { get; set; } = new List<Activity>();

        public List<ActivityParticipant> Participants { get; set; } = new List<ActivityParticipant>();

        public List<Tag> Tags { get; set; } = new List<Tag>();

        public List<ChecklistItem> ChecklistItems { get; set; } = new List<ChecklistItem>();

        public List<ActivityDependency> Dependencies { get; set; } = new List<ActivityDependency>();
    }
}
