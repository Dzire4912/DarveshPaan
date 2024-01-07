using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading.Channels;
using TAN.DomainModels.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TAN.DomainModels.Helpers
{

    [ExcludeFromCodeCoverage]
    public class CustomRoleManager<TRole> : RoleManager<TRole> where TRole : AspNetRoles
    {
        public CustomRoleManager(IRoleStore<TRole> store,
                                 IEnumerable<IRoleValidator<TRole>> roleValidators,
                                 ILookupNormalizer keyNormalizer,
                                 IdentityErrorDescriber errors,
                                 ILogger<RoleManager<TRole>> logger)
            : base(store, roleValidators, keyNormalizer, errors, logger)
        {
        }

        public override async Task<IdentityResult> CreateAsync(TRole role)
        {
            var errors = await ValidateRoleAsync(role);
            if (errors.Count() > 0)
            {
                return IdentityResult.Failed(errors.ToArray());
            }
            role.NormalizedName = NormalizeName(role.Name);
            await Store.CreateAsync(role, CancellationToken);
            return IdentityResult.Success;
        }
       
        public override async Task<IdentityResult> RemoveClaimAsync(TRole role, Claim claim)
        {
            var claimStore = GetClaimStore();
            await claimStore.RemoveClaimAsync(role, claim, CancellationToken.None);
            var result = await Store.UpdateAsync(role, CancellationToken.None);
            if (result.Succeeded)
            {
                return IdentityResult.Success;
            }
            else
            {
                return IdentityResult.Failed(result.Errors.ToArray());
            }
        }

        public override async Task<IdentityResult> AddClaimAsync(TRole role, Claim claim)
        {
            var errors = await ValidateRoleAsync(role);
            var claimStore = GetClaimStore();            

            if (errors.Count() > 0)
            {   foreach (var error in errors)
                {
                    if (error.Code != "DuplicateRoleName")
                    {
                        return IdentityResult.Failed(errors.ToArray());
                        
                    }
                }
            }
            await claimStore.AddClaimAsync(role, claim, CancellationToken.None);
            var result = await Store.UpdateAsync(role, CancellationToken.None);
            if (result.Succeeded)
            {
                return IdentityResult.Success;
            }
            else
            {
                return IdentityResult.Failed(errors.ToArray());
            }
        }
        private IRoleClaimStore<TRole> GetClaimStore()
        {
            var cast = Store as IRoleClaimStore<TRole>;
            if (cast == null)
            {
                throw new NotSupportedException("not suppoeted");
            }
            return cast;
        }
        protected virtual async Task<IEnumerable<IdentityError>> ValidateRoleAsync(TRole role)
        {
            var errors = new List<IdentityError>();

            // Retrieve only the custom RoleValidators
            var customRoleValidators = RoleValidators.OfType<CustomRoleValidator<TRole>>();

            foreach (var v in customRoleValidators)
            {
                var result = await v.ValidateAsync(this, role);
                errors.AddRange(result.Errors);
            }

            var existingRole = await RolesExistWithSameNameAndDifferentAppId(role);
            foreach (var exRole in existingRole)
            {
                if (exRole != null && exRole.ApplicationId == role.ApplicationId)
                {
                    var error = new IdentityError
                    {
                        Code = "DuplicateRoleName",
                        Description = $"Role name '{role.Name}' already exists with a different AppId."
                    };

                    errors.Add(error);
                }
            }

            return errors;
        }

        private async Task<List<TRole>> RolesExistWithSameNameAndDifferentAppId(TRole role)
        {
            var roleName = role.Name;
            var appId = role.ApplicationId;
            var roles = await Roles.Where(r => r.Name == roleName).ToListAsync();

            return roles;
        }

        public override async Task<IdentityResult> UpdateAsync(TRole role)
        {
            // Custom implementation for UpdateAsync
            // Provide necessary logic to update role
            var errors = new List<IdentityError>();
            var existingRole = await RolesExistWithSameNameAndDifferentAppId(role);
            role.NormalizedName = role.Name.ToUpper();
            foreach (var existsRole in existingRole)
            {
                if (existsRole != null && existsRole.ApplicationId == role.ApplicationId && existsRole.Id!=role.Id)
                {
                    var error = new IdentityError
                    {
                        Code = "DuplicateRoleName",
                        Description = $"Role name '{role.Name}' already exists with a different AppId."
                    };

                    errors.Add(error);
                }
            }
            if (errors.Count<=0)
            {
                return await Store.UpdateAsync(role, CancellationToken);
            }
            else
            {
                return IdentityResult.Failed(errors.ToArray());
            }
        }

        public override Task<IdentityResult> DeleteAsync(TRole role)
        {
            // Custom implementation for DeleteAsync
            // Provide necessary logic to delete role
            return base.DeleteAsync(role);
        }

        public override async Task<TRole> FindByIdAsync(string roleId)
        {
            // Custom implementation for FindByIdAsync
            // Provide necessary logic to find role by ID
            return await Store.FindByIdAsync(roleId, CancellationToken);
        }

        public override async Task<TRole> FindByNameAsync(string roleName)
        {
            // Custom implementation for FindByNameAsync
            // Provide necessary logic to find role by name
            return await Store.FindByNameAsync(roleName, CancellationToken.None);
        }
        private string NormalizeName(string roleName)
        {
            return roleName.Normalize().ToUpperInvariant();
        }

    }

}
