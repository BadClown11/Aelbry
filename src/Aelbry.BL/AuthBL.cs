using Aelbry.BL.Security;
using Aelbry.BO;
using Aelbry.BO.Auth;
using Aelbry.DAL;

namespace Aelbry.BL
{
    public class AuthBL
    {
        private readonly IJwtTokenService _jwtTokenService;

        public AuthBL(IJwtTokenService jwtTokenService)
        {
            _jwtTokenService = jwtTokenService;
        }

        public AuthResponse Login(string email, string password, string ipAddress)
        {
            User user;
            using (var dal = UserDAL.Instance)
            {
                user = dal.GetByEmail(email);
            }

            if (user == null || !user.IsActive || !PasswordHasher.Verify(password, user.PasswordHash))
            {
                throw new InvalidOperationException("Credenciales invalidas.");
            }

            return IssueTokens(user, ipAddress);
        }

        public AuthResponse Refresh(string refreshTokenValue, string ipAddress)
        {
            RefreshToken currentToken;
            using (var dal = AuthDAL.Instance)
            {
                currentToken = dal.GetByToken(refreshTokenValue);
            }

            if (currentToken == null || !currentToken.IsActive)
            {
                throw new InvalidOperationException("Refresh token invalido o expirado.");
            }

            User user;
            using (var dal = UserDAL.Instance)
            {
                user = dal.GetById(currentToken.UserId);
            }

            if (user == null || !user.IsActive)
            {
                throw new InvalidOperationException("Usuario inactivo.");
            }

            var (accessToken, accessTokenExpiresAt) = _jwtTokenService.GenerateAccessToken(user);
            var newRefreshToken = new RefreshToken
            {
                UserId = user.UserId,
                Token = _jwtTokenService.GenerateRefreshTokenValue(),
                ExpiresAt = _jwtTokenService.GetRefreshTokenExpiration(),
                CreatedByIp = ipAddress,
            };

            using (var dal = AuthDAL.Instance)
            {
                dal.RotateRefreshToken(refreshTokenValue, newRefreshToken, ipAddress);
            }

            return BuildAuthResponse(user, accessToken, accessTokenExpiresAt, newRefreshToken);
        }

        public void Logout(string refreshTokenValue, string ipAddress)
        {
            using (var dal = AuthDAL.Instance)
            {
                dal.RevokeToken(refreshTokenValue, ipAddress);
            }
        }

        private AuthResponse IssueTokens(User user, string ipAddress)
        {
            var (accessToken, accessTokenExpiresAt) = _jwtTokenService.GenerateAccessToken(user);
            var refreshToken = new RefreshToken
            {
                UserId = user.UserId,
                Token = _jwtTokenService.GenerateRefreshTokenValue(),
                ExpiresAt = _jwtTokenService.GetRefreshTokenExpiration(),
                CreatedByIp = ipAddress,
            };

            using (var dal = AuthDAL.Instance)
            {
                dal.SaveRefreshToken(refreshToken);
            }

            return BuildAuthResponse(user, accessToken, accessTokenExpiresAt, refreshToken);
        }

        private static AuthResponse BuildAuthResponse(User user, string accessToken, DateTime accessTokenExpiresAt, RefreshToken refreshToken)
        {
            return new AuthResponse
            {
                UserId = user.UserId,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                Roles = user.Roles.Select(r => r.Name).ToList(),
                Permissions = user.Permissions.Select(p => p.Code).Distinct().ToList(),
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessTokenExpiresAt,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiresAt = refreshToken.ExpiresAt,
            };
        }
    }
}
