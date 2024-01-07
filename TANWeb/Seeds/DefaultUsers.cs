using Microsoft.AspNetCore.Identity;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.Helpers.AppSettings;
using TAN.Repository.Abstractions;
using TAN.Repository.Implementations;

namespace TANWeb.Seeds
{
    [ExcludeFromCodeCoverage]
    public static class DefaultUsers
    {

        public static async Task SeedBasicUserAsync(UserManager<AspNetUser> userManager,IUnitOfWork unitOfWork)
        {
            //Seed Default User
            var defaultUser = new AspNetUser
            {
                UserName = DefaultUserAppSettings.UserName,
                Email = DefaultUserAppSettings.UserName,
                EmailConfirmed = DefaultUserAppSettings.EmailConfirmed,
                PhoneNumberConfirmed = DefaultUserAppSettings.PhoneNumberConfirmed,
                IsActive= true,
                TwoFactorEnabled = DefaultUserAppSettings.TwoFactorEnabled,
                DefaultAppId = 1,
                FirstName = DefaultUserAppSettings.FirstName,
                LastName = DefaultUserAppSettings.LastName,
                UserType = 1
            };
            if (userManager.Users.All(u => u.Id != defaultUser.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    try
                    {
                        await userManager.CreateAsync(defaultUser, DefaultUserAppSettings.Password);
                        await userManager.AddToRoleAsync(defaultUser, Roles.User.ToString());
                        await userManager.AddClaimAsync(defaultUser, new Claim("UserName", defaultUser.FirstName + " " + defaultUser.LastName));
                        UserApplication userApplication = new UserApplication();
                        userApplication.ApplicationId = defaultUser.DefaultAppId;
                        userApplication.UserId = defaultUser.Id;
                        unitOfWork.UserApplicationRepo.Add(userApplication);
                        unitOfWork.SaveChanges();
                    }
                    catch (Exception ex) { }
                }
            }
        }

        public static async Task SeedAdminUserAsync(UserManager<AspNetUser> userManager,IUnitOfWork unitOfWork)
        {
            //Seed Admin User
            var defaultUser = new AspNetUser
            {
                UserName = AdminUserAppSettings.UserName,
                Email = AdminUserAppSettings.UserName,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsActive = true,
                DefaultAppId=1,
                FirstName = AdminUserAppSettings.FirstName,
                LastName = AdminUserAppSettings.LastName,
                UserType = 1
            };
            if (userManager.Users.All(u => u.Id != defaultUser.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    try
                    {
                        await userManager.CreateAsync(defaultUser, AdminUserAppSettings.Password);
                        await userManager.AddToRoleAsync(defaultUser, Roles.Admin.ToString());
                        await userManager.AddClaimAsync(defaultUser, new Claim("UserName", defaultUser.FirstName + " " + defaultUser.LastName));
                        UserApplication userApplication = new UserApplication();
                        userApplication.ApplicationId = defaultUser.DefaultAppId;
                        userApplication.UserId = defaultUser.Id;
                        unitOfWork.UserApplicationRepo.Add(userApplication);
                        unitOfWork.SaveChanges();
                    }
                    catch (Exception ex) { }
                }
            }
        }

        public static async Task SeedSuperAdminAsync(UserManager<AspNetUser> userManager, RoleManager<AspNetRoles> roleManager, IUnitOfWork unitOfWork)
        {
            //Seed SuperAdmin User
            var defaultUser = new AspNetUser
            {
                UserName = SuperAdminAppSettings.UserName,
                Email = SuperAdminAppSettings.Email,
                EmailConfirmed = SuperAdminAppSettings.EmailConfirmed,
                PhoneNumberConfirmed = SuperAdminAppSettings.PhoneNumberConfirmed,
                IsActive= true,
                TwoFactorEnabled = SuperAdminAppSettings.TwoFactorEnabled,
                DefaultAppId = 1, // need to change
                FirstName = SuperAdminAppSettings.FirstName,
                LastName = SuperAdminAppSettings.LastName,
                UserType = 1
            };
            if (userManager.Users.All(u => u.Id != defaultUser.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    try
                    {
                        await userManager.CreateAsync(defaultUser, SuperAdminAppSettings.Password);
                        await userManager.AddToRoleAsync(defaultUser, Roles.SuperAdmin.ToString());
                        await userManager.AddClaimAsync(defaultUser,new Claim("UserName", defaultUser.FirstName + " " + defaultUser.LastName));
                        //populate UserApplication
                        var apps= unitOfWork.ApplicationRepo.GetAll().ToList();
                        foreach (var app in apps)
                        {
                            UserApplication userApplication = new UserApplication();
                            userApplication.ApplicationId = app.ApplicationId;
                            userApplication.UserId = defaultUser.Id;
                            unitOfWork.UserApplicationRepo.Add(userApplication);
                            unitOfWork.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {//Log
                    }
                }
                await roleManager.SeedClaimsForSuperAdmin(unitOfWork);
            }
        }

        private async static Task SeedClaimsForSuperAdmin(this RoleManager<AspNetRoles> roleManager, IUnitOfWork unitOfWork)
        {
            var adminRole = await roleManager.FindByNameAsync("SuperAdmin");
            var moduleList = Enum.GetValues(typeof(AllModules))
               .Cast<AllModules>()
              .Select(v => v.ToString())
              .ToList();
            await roleManager.AddPermissionClaim(adminRole, moduleList, unitOfWork);
        }

        public static async Task AddPermissionClaim(this RoleManager<AspNetRoles> roleManager, AspNetRoles role, List<string> ModuleList, IUnitOfWork unitOfWork)

        {
            var allClaims = await roleManager.GetClaimsAsync(role);
            // List<string> allPermissions = new List<string>();
            foreach (var item in ModuleList)
            {
                List<string> appname = new List<string>();
                var strings = unitOfWork.ApplicationRepo.GetAll().ToList();
                foreach (var app in strings)
                {
                    appname.Add(app.Name);
                }
                List<string> allPermissions = Permissions.GeneratePermissionsForModule(item, appname);
                foreach (var permission in allPermissions)
                {
                    if (!allClaims.Any(a => a.Type == "Permission" && a.Value == permission))
                    {
                        await roleManager.AddClaimAsync(role, new Claim("Permission", permission));
                    }
                }
            }
        }
    }
}
