using Microsoft.AspNetCore.Identity;
using System.Diagnostics.CodeAnalysis;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.Helpers.AppSettings;

namespace TANWeb.Seeds
{
	[ExcludeFromCodeCoverage]
    public static class SeedRoles
    {
        public static async Task SeedAsync(UserManager<AspNetUser> userManager, RoleManager<AspNetRoles> roleManager)
        {
			//Seed SuperAdmin Role
			var superAdmin = new AspNetRoles
			{
				ApplicationId = "0",
				CreatedAt = null,
				Name = Roles.SuperAdmin.ToString(),
				RoleType = RoleTypes.TANApplicationUser.ToString(),
				CreatedBy = null,
				IsActive = true
			};

			// Seed Admin Role
			var admin = new AspNetRoles
			{
				ApplicationId = "0",
				CreatedAt = null,
				Name = Roles.Admin.ToString(),
				RoleType = RoleTypes.TANApplicationUser.ToString(),
				CreatedBy = null,
				IsActive = true
			};

			// Seed User Role
			var user = new AspNetRoles
			{
				ApplicationId = "0",
				CreatedAt = null,
				Name = Roles.User.ToString(),
				RoleType = RoleTypes.TANApplicationUser.ToString(),
				CreatedBy = null,
				IsActive = true
			};
			try { 
			// Add Roles (SuperAdmin, Admin, User)
			await roleManager.CreateAsync(superAdmin);
			await roleManager.CreateAsync(admin);
			await roleManager.CreateAsync(user);
            }
            catch (Exception ex) 
			{
				//Log
			}
        }
    }
}
