using Microsoft.AspNetCore.Authorization;
using System.Diagnostics.CodeAnalysis;

namespace TANWeb.Permission
{
    [ExcludeFromCodeCoverage]
    internal class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; private set; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}
