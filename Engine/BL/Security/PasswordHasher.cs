namespace Aelbry.BL.Security
{
    /// <summary>
    /// Hashing de contrasenas con BCrypt (work factor 12). No forma parte del acceso a datos,
    /// por lo que no esta sujeto a las reglas de DAL/Stored Procedures.
    /// </summary>
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
