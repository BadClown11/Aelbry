using Aelbry.BL.Security;
using Xunit;

namespace Aelbry.Tests
{
    public class PasswordHasherTests
    {
        [Fact]
        public void Hash_ThenVerify_WithCorrectPassword_ReturnsTrue()
        {
            string hash = PasswordHasher.Hash("Admin#12345");

            Assert.True(PasswordHasher.Verify("Admin#12345", hash));
        }

        [Fact]
        public void Verify_WithWrongPassword_ReturnsFalse()
        {
            string hash = PasswordHasher.Hash("Admin#12345");

            Assert.False(PasswordHasher.Verify("otra-contrasena", hash));
        }

        [Fact]
        public void Hash_ProducesDifferentHash_ForSamePasswordEachTime()
        {
            string hash1 = PasswordHasher.Hash("Admin#12345");
            string hash2 = PasswordHasher.Hash("Admin#12345");

            Assert.NotEqual(hash1, hash2);
        }
    }
}
