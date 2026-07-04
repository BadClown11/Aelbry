using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Aelbry.BL.Email
{
    /// <summary>
    /// Envio de correos transaccionales via SMTP (System.Net.Mail, ya incluido en .NET, sin
    /// paquetes adicionales). Gemini:ApiKey y Email:SmtpHost siguen el mismo patron: quedan
    /// vacios en appsettings hasta que el usuario configure sus propias credenciales.
    /// </summary>
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailOptions _options;

        public SmtpEmailSender(IOptions<EmailOptions> options)
        {
            _options = options.Value;
        }

        public Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            return SendWithAttachmentAsync(toEmail, subject, htmlBody, null, null, null);
        }

        public async Task SendWithAttachmentAsync(string toEmail, string subject, string htmlBody, byte[] attachmentBytes, string attachmentFileName, string attachmentContentType)
        {
            if (string.IsNullOrWhiteSpace(_options.SmtpHost))
            {
                throw new InvalidOperationException(
                    "El servidor SMTP no esta configurado. Agrega Email:SmtpHost/Username/Password en appsettings o user-secrets.");
            }

            using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
            {
                Credentials = new NetworkCredential(_options.Username, _options.Password),
                EnableSsl = _options.EnableSsl,
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_options.FromAddress, _options.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true,
            };
            message.To.Add(toEmail);

            MemoryStream attachmentStream = null;
            try
            {
                if (attachmentBytes != null)
                {
                    attachmentStream = new MemoryStream(attachmentBytes);
                    message.Attachments.Add(new Attachment(attachmentStream, attachmentFileName, attachmentContentType));
                }

                await client.SendMailAsync(message);
            }
            finally
            {
                attachmentStream?.Dispose();
            }
        }
    }
}
