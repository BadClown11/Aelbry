using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Aelbry.BO;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Aelbry.BL.Security
{
    public class JwtTokenService : IJwtTokenService
    {
        public const string PermissionClaimType = "permission";
        public const string CompanyClaimType = "company_id";

        private readonly JwtOptions _options;

        public JwtTokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        public (string Token, DateTime ExpiresAt) GenerateAccessToken(User user)
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(CompanyClaimType, user.CompanyId.ToString()),
            };

            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            foreach (var permission in user.Permissions)
            {
                claims.Add(new Claim(PermissionClaimType, permission.Code));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }

        public string GenerateRefreshTokenValue()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }

        public DateTime GetRefreshTokenExpiration()
        {
            return DateTime.UtcNow.AddDays(_options.RefreshTokenDays);
        }

        public ClaimsPrincipal ValidateAccessToken(string token)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _options.Issuer,
                ValidateAudience = true,
                ValidAudience = _options.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            };

            try
            {
                return new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);
            }
            catch (SecurityTokenException)
            {
                return null;
            }
        }
    }
}
