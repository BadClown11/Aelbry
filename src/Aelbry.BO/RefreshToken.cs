namespace Aelbry.BO
{
    public class RefreshToken
    {
        public int RefreshTokenId { get; set; }

        public int UserId { get; set; }

        public string Token { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedByIp { get; set; }

        public DateTime? RevokedAt { get; set; }

        public string ReplacedByToken { get; set; }

        public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
    }
}
