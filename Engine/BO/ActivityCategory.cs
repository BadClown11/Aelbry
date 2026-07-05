using Aelbry.BO.Common;

namespace Aelbry.BO
{
    public class ActivityCategory : AuditableEntity
    {
        public int ActivityCategoryId { get; set; }

        public int CompanyId { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }
    }
}
