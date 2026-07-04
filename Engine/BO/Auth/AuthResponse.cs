namespace Aelbry.BO.Auth
{
    public class AuthResponse
    {
        public int UserId { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public List<string> Roles { get; set; } = new List<string>();

        public List<string> Permissions { get; set; } = new List<string>();

        public string AccessToken { get; set; }

        public DateTime AccessTokenExpiresAt { get; set; }

        public string RefreshToken { get; set; }

        public DateTime RefreshTokenExpiresAt { get; set; }
    }
}
