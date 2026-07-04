using Microsoft.AspNetCore.Authorization;

namespace Aelbry.Web.Security
{
    /// <summary>
    /// Genera dinamicamente una policy de autorizacion por cada permiso granular
    /// (ej. [Authorize(Policy = "Permission:USERS_VIEW")]) sin necesidad de registrarlas una a una.
    /// </summary>
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        public const string PolicyPrefix = "Permission:";

        private readonly DefaultAuthorizationPolicyProvider _fallbackProvider;

        public PermissionPolicyProvider(Microsoft.Extensions.Options.IOptions<AuthorizationOptions> options)
        {
            _fallbackProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy> GetFallbackPolicyAsync() => _fallbackProvider.GetFallbackPolicyAsync();

        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(PolicyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                string permissionCode = policyName[PolicyPrefix.Length..];

                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new PermissionRequirement(permissionCode))
                    .Build();

                return Task.FromResult(policy);
            }

            return _fallbackProvider.GetPolicyAsync(policyName);
        }
    }
}
