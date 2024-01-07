using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using TAN.DomainModels.Entities;
using TAN.Repository.Abstractions;
using TAN.Repository.Implementations;

namespace TANWeb.Permission
{
    [ExcludeFromCodeCoverage]
    internal class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        UserManager<AspNetUser> _userManager;
        RoleManager<AspNetRoles> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        public PermissionAuthorizationHandler(UserManager<AspNetUser> userManager, RoleManager<AspNetRoles> roleManager,IUnitOfWork unitOfWork,IMemoryCache memoryCache)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _cache = memoryCache;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            try
            {
                if (context.User == null)
                {
                    return;
                }
                var user = _cache.Get("CurrentUser"+context.User.Identity.Name) as AspNetUser;
                if (user == null)
                {
                     user = await _userManager.GetUserAsync(context.User);
                    _cache.Set("CurrentUser"+user.UserName, user, TimeSpan.FromDays(1));
                }
                var userRoles = _cache.Get("UserRoles"+user.Id) as List<AspNetUserRoles>;
                if (userRoles == null)
                {
                     userRoles = _unitOfWork.AspNetUserRoleRepo.GetAll().Where(x => x.UserId == user.Id).ToList();
                    _cache.Set("UserRoles"+user.Id, userRoles, TimeSpan.FromDays(1));
                }

                var userRolesList =_cache .Get("userRolesList"+user.Id) as List<AspNetRoles>;
                if (userRolesList == null)
                {
                    List<AspNetRoles>newUserRoleList= new List<AspNetRoles>();
                    foreach (var iten in userRoles)
                    {
                        var role =(AspNetRoles) _roleManager.Roles.Where(x => x.Id == iten.RoleId).FirstOrDefault();
                        newUserRoleList.Add(role);
                    }
                    userRolesList= newUserRoleList;
                    _cache.Set("userRolesList" + user.Id, userRolesList, TimeSpan.FromDays(1));
                }
               

                // var userRoleNames = await _userManager.GetRolesAsync(user);
                //      var userRoles = _roleManager.Roles.Where(x => userRoleNames.Contains(x.Name)).ToList();
                
                foreach (var role in userRolesList)
                {

                    var roleClaims = _cache.Get(role.Id) as List<System.Security.Claims.Claim>;
                    if (roleClaims == null)
                    {
                         roleClaims = (List<System.Security.Claims.Claim>?)await _roleManager.GetClaimsAsync(role);
                        _cache.Set(role.Id, roleClaims, TimeSpan.FromDays(1));
                    }
                    var permissions = roleClaims.Where(x => x.Type == "Permission" &&
                                                            x.Value == requirement.Permission &&
                                                            x.Issuer == "LOCAL AUTHORITY")
                                                .Select(x => x.Value);
                    if (permissions.Any())
                    {
                        context.Succeed(requirement);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                Log.Error(ex, "An Error Occured :{ErrorMsg}",ex.Message);
                Log.Error("An Error Occured :{ErrorMsg}", ex.Message);
            }
        }
    }
}
