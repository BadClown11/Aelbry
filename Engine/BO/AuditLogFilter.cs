namespace Aelbry.BO
{
    public class AuditLogFilter
    {
        public int CompanyId { get; set; }

        public string Module { get; set; }

        public int? UserId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
