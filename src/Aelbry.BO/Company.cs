using Aelbry.BO.Common;

namespace Aelbry.BO
{
    public class Company : AuditableEntity
    {
        public int CompanyId { get; set; }

        public string Name { get; set; }

        public string LegalTaxId { get; set; }

        public string LogoUrl { get; set; }

        public string TimeZone { get; set; }

        public bool IsActive { get; set; }
    }
}
