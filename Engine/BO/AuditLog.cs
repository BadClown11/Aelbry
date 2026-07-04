namespace Aelbry.BO
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }

        public int CompanyId { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }

        public string IPAddress { get; set; }

        public string Module { get; set; }

        public string Action { get; set; }

        public int? EntityId { get; set; }

        public string DataBefore { get; set; }

        public string DataAfter { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
