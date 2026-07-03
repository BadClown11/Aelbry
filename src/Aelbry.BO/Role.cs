using Aelbry.BO.Common;

namespace Aelbry.BO
{
    public class Role : AuditableEntity
    {
        public int RoleId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsSystemDefault { get; set; }

        public bool IsActive { get; set; }

        public List<Permission> Permissions { get; set; } = new List<Permission>();
    }
}
