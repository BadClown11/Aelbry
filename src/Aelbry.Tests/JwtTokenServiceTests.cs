using System.Security.Claims;
using Aelbry.BL.Security;
using Aelbry.BO;
using Microsoft.Extensions.Options;
using Xunit;

namespace Aelbry.Tests
{
    public class JwtTokenServiceTests
    {
        private static JwtTokenService CreateService()
        {
            var options = Options.Create(new JwtOptions
            {
                Key = "clave-de-prueba-de-al-menos-32-caracteres!",
                Issuer = "Aelbry.Tests",
                Audience = "Aelbry.Tests.Clients",
                AccessTokenMinutes = 15,
                RefreshTokenDays = 7,
            });

            return new JwtTokenService(options);
        }

        private static User CreateUser()
        {
            return new User
            {
                UserId = 10,
                CompanyId = 1,
                FirstName = "Ada",
                LastName = "Lovelace",
                Email = "ada@aelbry.local",
                Roles = new List<Role> { new Role { RoleId = 1, Name = "Admin" } },
                Permissions = new List<Permission> { new Permission { PermissionId = 1, Code = "USERS_VIEW" } },
            };
        }

        [Fact]
        public void GenerateAccessToken_ProducesTokenValidatedByTheSameService()
        {
            var service = CreateService();
            var user = CreateUser();

            var (token, expiresAt) = service.GenerateAccessToken(user);
            var principal = service.ValidateAccessToken(token);

            Assert.NotNull(principal);
            Assert.True(expiresAt > DateTime.UtcNow);
            Assert.Equal("10", principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            Assert.Contains(principal.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
            Assert.Contains(principal.Claims, c => c.Type == JwtTokenService.PermissionClaimType && c.Value == "USERS_VIEW");
        }

        [Fact]
        public void ValidateAccessToken_ReturnsNull_ForTamperedToken()
        {
            var service = CreateService();
            var (token, _) = service.GenerateAccessToken(CreateUser());

            var tamperedToken = token[..^2] + "xx";

            Assert.Null(service.ValidateAccessToken(tamperedToken));
        }

        [Fact]
        public void GenerateRefreshTokenValue_ProducesUniqueValues()
        {
            var service = CreateService();

            string first = service.GenerateRefreshTokenValue();
            string second = service.GenerateRefreshTokenValue();

            Assert.NotEqual(first, second);
        }

        [Fact]
        public void GetRefreshTokenExpiration_IsInTheFuture()
        {
            var service = CreateService();

            Assert.True(service.GetRefreshTokenExpiration() > DateTime.UtcNow);
        }
    }
}
