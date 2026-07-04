using Aelbry.BO.Common;

namespace Aelbry.BO
{
    public class Project : AuditableEntity
    {
        public int ProjectId { get; set; }

        public int CompanyId { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string ColorHex { get; set; }

        public string CoverImageUrl { get; set; }

        public string ClientName { get; set; }

        public int ProjectStatusId { get; set; }

        public string ProjectStatusName { get; set; }

        public ProjectPriority Priority { get; set; }

        public ProjectRiskLevel RiskLevel { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public decimal? Budget { get; set; }

        public decimal EstimatedHours { get; set; }

        public decimal WorkedHours { get; set; }

        public decimal ProgressPercentage { get; set; }

        public int ProjectManagerId { get; set; }

        public string ProjectManagerName { get; set; }

        public bool IsActive { get; set; }

        public List<ProjectMember> Members { get; set; } = new List<ProjectMember>();

        public List<Tag> Tags { get; set; } = new List<Tag>();
    }
}
