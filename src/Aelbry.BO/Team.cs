using Aelbry.BO.Common;

namespace Aelbry.BO
{
    public class Team : AuditableEntity
    {
        public int TeamId { get; set; }

        public int DepartmentId { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }
    }
}
