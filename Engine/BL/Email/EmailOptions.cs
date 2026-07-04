namespace Aelbry.BL.Email
{
    public class EmailOptions
    {
        public const string SectionName = "Email";

        public string SmtpHost { get; set; } = string.Empty;

        public int SmtpPort { get; set; } = 587;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string FromAddress { get; set; } = "no-reply@aelbry.local";

        public string FromName { get; set; } = "Aelbry";

        public bool EnableSsl { get; set; } = true;
    }
}
