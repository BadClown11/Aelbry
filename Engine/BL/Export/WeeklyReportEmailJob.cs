using Aelbry.BL.Email;
using Aelbry.BO;
using Aelbry.BO.Reports;
using Aelbry.DAL;

namespace Aelbry.BL.Export
{
    /// <summary>
    /// Job recurrente (disparado por Hangfire) que envia el reporte semanal en Excel a todos
    /// los usuarios con permiso REPORTS_VIEW de cada empresa activa. Se salta empresas sin
    /// actividades y usuarios individuales cuyo envio de correo falla (SMTP no configurado,
    /// direccion invalida, etc.) para no abortar el resto del lote.
    /// </summary>
    public class WeeklyReportEmailJob
    {
        private readonly ReportBL _reportBL;
        private readonly IEmailSender _emailSender;

        public WeeklyReportEmailJob(ReportBL reportBL, IEmailSender emailSender)
        {
            _reportBL = reportBL;
            _emailSender = emailSender;
        }

        public async Task RunAsync()
        {
            List<Company> companies;
            using (var companyDal = CompanyDAL.Instance)
            {
                companies = companyDal.GetAll().Where(c => c.IsActive).ToList();
            }

            foreach (var company in companies)
            {
                var filter = new ReportFilter { CompanyId = company.CompanyId };
                var rows = _reportBL.GetWeeklyReport(filter);
                if (rows.Count == 0)
                {
                    continue;
                }

                byte[] excelBytes = _reportBL.ExportWeeklyReportToExcel(filter, $"Reporte semanal - {company.Name}");
                var recipients = GetReportRecipients(company.CompanyId);

                foreach (var recipient in recipients)
                {
                    try
                    {
                        await _emailSender.SendWithAttachmentAsync(
                            recipient.Email,
                            $"Reporte semanal de actividades - {company.Name}",
                            $"<p>Adjunto el reporte semanal de actividades de <strong>{company.Name}</strong>.</p>",
                            excelBytes,
                            "ReporteSemanal.xlsx",
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                    }
                    catch (InvalidOperationException)
                    {
                        // SMTP no configurado (Email:SmtpHost vacio) u otro problema de envio: se omite este destinatario.
                    }
                }
            }
        }

        private static List<User> GetReportRecipients(int companyId)
        {
            using (var userDal = UserDAL.Instance)
            {
                return userDal.GetByCompany(companyId)
                    .Where(u => !u.IsDeleted && userDal.GetPermissions(u.UserId).Any(p => p.Code == "REPORTS_VIEW"))
                    .ToList();
            }
        }
    }
}
