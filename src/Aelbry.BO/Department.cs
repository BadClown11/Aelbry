using Aelbry.BO.Common;

namespace Aelbry.BO
{
    public class Department : AuditableEntity
    {
        public int DepartmentId { get; set; }

        public int CompanyId { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }
    }
}
