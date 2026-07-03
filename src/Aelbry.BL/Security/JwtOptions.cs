namespace Aelbry.BL.Security
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        public string Key { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public int AccessTokenMinutes { get; set; } = 15;

        public int RefreshTokenDays { get; set; } = 7;
    }
}
