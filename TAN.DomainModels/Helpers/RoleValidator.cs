using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;

namespace TAN.DomainModels.Helpers
{

    //public class RoleValidator : RoleValidator<AspNetRoles>
    //{
    //    private IdentityErrorDescriber Describer { get; set; }

    //    public RoleValidator() : base()
    //    {

    //    }
    //    public override async Task<IdentityResult> ValidateAsync(RoleManager<AspNetRoles> manager, AspNetRoles role)
    //    {
    //        if (manager == null)
    //        {
    //            throw new ArgumentNullException(nameof(manager));
    //        }
    //        if (role == null)
    //        {
    //            throw new ArgumentNullException(nameof(role));
    //        }
    //        var errors = new List<IdentityError>();
    //        await ValidateRoleName(manager, role, errors);
    //        if (errors.Count > 0)
    //        {
    //            return IdentityResult.Failed(errors.ToArray());
    //        }
    //        return IdentityResult.Success;
    //    }
    //    private async Task ValidateRoleName(RoleManager<AspNetRoles> manager, AspNetRoles role,
    //    ICollection<IdentityError> errors)
    //    {
    //        var roleName = await manager.GetRoleNameAsync(role);
    //        if (string.IsNullOrWhiteSpace(roleName))
    //        {
    //            errors.Add(Describer.InvalidRoleName(roleName));
    //        }
    //        else
    //        {
    //            var owner = await manager.FindByNameAsync(roleName);
    //            if (owner != null
    //                && owner.ApplicationId == role.ApplicationId
    //                && !string.Equals(await manager.GetRoleIdAsync(owner), await manager.GetRoleIdAsync(role)))
    //            {
    //                try { 
    //                errors.Add(Describer.DuplicateRoleName(roleName));
    //                }
    //                catch (Exception ex) { }
    //            }
    //        }
    //    }
    //}
}




