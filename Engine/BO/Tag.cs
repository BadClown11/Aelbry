using Aelbry.BO.Common;

namespace Aelbry.BO
{
    public class Tag : AuditableEntity
    {
        public int TagId { get; set; }

        public int CompanyId { get; set; }

        public string Name { get; set; }

        public string ColorHex { get; set; }

        public bool IsActive { get; set; }
    }
}
