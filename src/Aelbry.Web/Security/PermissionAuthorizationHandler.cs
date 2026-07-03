using Aelbry.BL.Security;
using Microsoft.AspNetCore.Authorization;

namespace Aelbry.Web.Security
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            bool hasPermission = context.User.Claims.Any(c =>
                c.Type == JwtTokenService.PermissionClaimType && c.Value == requirement.PermissionCode);

            if (hasPermission)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
