using System.Security.Claims;
using Aelbry.BO;

namespace Aelbry.BL.Security
{
    public interface IJwtTokenService
    {
        (string Token, DateTime ExpiresAt) GenerateAccessToken(User user);

        string GenerateRefreshTokenValue();

        DateTime GetRefreshTokenExpiration();

        ClaimsPrincipal ValidateAccessToken(string token);
    }
}
