namespace Aelbry.BL.Security
{

    public static class PasswordHasher
    {
        private const int WorkFactor = 12;

        public static string Hash(string plainPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(plainPassword, WorkFactor);
        }

        public static bool Verify(string plainPassword, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(plainPassword, passwordHash);
        }
    }
}
