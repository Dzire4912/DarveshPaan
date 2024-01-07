using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using System.Diagnostics.CodeAnalysis;

namespace TAN.DomainModels.Helpers
{

    [ExcludeFromCodeCoverage]
    public class CustomRoleValidator<TRole> : RoleValidator<TRole> where TRole : AspNetRoles
    {
        public override async Task<IdentityResult> ValidateAsync(RoleManager<TRole> manager, TRole role)
        {
            // Skip role name uniqueness check
            var result = await base.ValidateAsync(manager, role);

            if (result.Succeeded)
            {
                return IdentityResult.Success;
            }

            var errors = result.Errors.ToList();
            errors.RemoveAll(error => error.Code == "DuplicateRoleName");
            return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
        }
    }

}
