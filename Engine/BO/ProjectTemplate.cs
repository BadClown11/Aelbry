using Aelbry.BO.Common;

namespace Aelbry.BO
{
    public class ProjectTemplate : AuditableEntity
    {
        public int ProjectTemplateId { get; set; }

        public int CompanyId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public ProjectPriority DefaultPriority { get; set; }

        public decimal DefaultEstimatedHours { get; set; }

        public bool IsActive { get; set; }
    }
}
