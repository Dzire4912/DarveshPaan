using Microsoft.AspNetCore.Identity;
using TAN.DomainModels.Entities;
using TAN.DomainModels.ViewModels;
using System.Reflection;
using System.Security.Claims;
using System.Diagnostics.CodeAnalysis;

namespace TANWeb.Helpers
{
    public static class ClaimsHelper
    {
        [ExcludeFromCodeCoverage]
        public static void GetPermissions(this List<RoleClaimsViewModel> allPermissions, Type policy, string roleId)
        {
            FieldInfo[] fields = policy.GetFields(BindingFlags.Static | BindingFlags.Public);

            foreach (FieldInfo fi in fields)
            {
                allPermissions.Add(new RoleClaimsViewModel { Value = fi.GetValue(null).ToString(), Type = "Permissions" });
            }
        }

        public static async Task AddPermissionClaim(this RoleManager<AspNetRoles> roleManager, AspNetRoles role, string permission)
        {
            var allClaims = await roleManager.GetClaimsAsync(role);
            if (!allClaims.Any(a => a.Type == "Permission" && a.Value == permission))
            {
                try { 
                  var x= await roleManager.AddClaimAsync(role, new Claim("Permission", permission));
                }
                catch (Exception ex) { }
            }
        }
    }
}
