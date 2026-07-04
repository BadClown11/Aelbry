using Aelbry.BO.Common;

namespace Aelbry.BO
{
    public class User : AuditableEntity
    {
        public int UserId { get; set; }

        public int CompanyId { get; set; }

        public int? DepartmentId { get; set; }

        public int? TeamId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public string JobTitle { get; set; }

        public string PhotoUrl { get; set; }

        public string TimeZone { get; set; }

        public string WorkScheduleJson { get; set; }

        public string ProfileColor { get; set; }

        public bool IsActive { get; set; }

        public List<Role> Roles { get; set; } = new List<Role>();

        public List<Permission> Permissions { get; set; } = new List<Permission>();
    }
}
