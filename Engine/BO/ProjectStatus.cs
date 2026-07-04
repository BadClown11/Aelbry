using Aelbry.BO.Common;

namespace Aelbry.BO
{
    public class ProjectStatus : AuditableEntity
    {
        public int ProjectStatusId { get; set; }

        public int CompanyId { get; set; }

        public string Name { get; set; }

        public string ColorHex { get; set; }

        public int Sequence { get; set; }

        public bool IsFinal { get; set; }

        public bool IsActive { get; set; }
    }
}
