using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using TAN.DomainModels.Entities;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TANWeb.Controllers;
using TANWeb.Helpers;
using TANWeb.Resources;

namespace TANWeb.Areas.TANUserManagement.Controllers
{
    [Area("TANUserManagement")]
    [AllowAnonymous]
    public class PermissionController : BaseController
    {
        private readonly RoleManager<AspNetRoles> _roleManager;
        private readonly IUnitOfWork _uow;
        private readonly IMemoryCache _memoryCache;
        public PermissionController(IUnitOfWork uow, RoleManager<AspNetRoles> roleManager, IMemoryCache memoryCache) : base(uow)
        {
            _roleManager = roleManager;
            _uow = uow;
            _memoryCache = memoryCache;
        }
        [Route("PBJ/Permission")]
        [Route("PBJ/Permission/Index")]
        [Route("PBJ/Permission/Index/{id?}")]
        public async Task<ActionResult> Index(string roleId, int ModuleId = 0, int AppId = 0)
        {
            var model = new PermissionViewModel();
            try
            {
                if (roleId != null)
                {
                    var roleDetails = _roleManager.Roles.Where(x => x.Id == roleId).FirstOrDefault();
                    if (roleDetails != null)
                    {
                        AppId = Convert.ToInt32(roleDetails.ApplicationId);
                    }
                }
                //Get List of Modules for Dropdown
                if (AppId != 0)
                {
                    model.ModuleList = _uow.ModuleRepo.GetAll().Where(x => x.ApplicationId == AppId).Select(p => new Itemlist
                    {
                        Text = p.Name,
                        Value = p.ModuleId
                    }).ToList();
                    model.RoleList = _roleManager.Roles.Where(s => s.Name != "SuperAdmin" && s.ApplicationId == AppId.ToString()).Select(p => new roleItemlist
                    {
                        Text = p.Name,
                        Value = p.Id
                    }).ToList();
                }
                else
                {
                    model.ModuleList = _uow.ModuleRepo.GetAll().Select(p => new Itemlist
                    {
                        Text = p.Name,
                        Value = p.ModuleId
                    }).ToList();
                    model.RoleList = _roleManager.Roles.Where(s => s.Name != "SuperAdmin").Select(p => new roleItemlist
                    {
                        Text = p.Name,
                        Value = p.Id
                    }).ToList();
                }
                //Get List of Application for Dropdown
                model.AppList = _uow.ApplicationRepo.GetAll().Select(p => new appItemlist
                {
                    Text = p.Name,
                    Value = p.ApplicationId
                }).ToList();
                foreach (var item in model.AppList)
                {
                    if (item.Value == AppId)
                    {
                        item.Selected = true;
                    }
                }



                //Getting permission list from constants(add new module roles below)         
                var allPermissions = new List<RoleClaimsViewModel>();
                allPermissions = _uow.PermissionsRepo.GetPermissions(ModuleId, AppId).Select(p => new RoleClaimsViewModel
                {
                    Type = "Permissions",
                    Value = p.Name,
                    Selected = false,
                    ModuleId = p.ModuleId
                }).ToList();
                var role = await _roleManager.FindByIdAsync(roleId);
                model.RoleId = roleId;
                model.ModuleId = ModuleId;
                model.RoleName = role != null ? role.Name : null;
                var claims = await _roleManager.GetClaimsAsync(role);
                var allClaimValues = allPermissions.Select(a => a.Value).ToList();
                var roleClaimValues = claims.Select(a => a.Value).ToList();
                var authorizedClaims = allClaimValues.Intersect(roleClaimValues).ToList();
                //mark peremissions of the roles as selected
                foreach (var permission in allPermissions)
                {
                    if (authorizedClaims.Any(a => a == permission.Value))
                    {
                        permission.Selected = true;
                    }
                }
                if (ModuleId != 0)
                {
                    model.RoleClaims = allPermissions;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Permission Controller:{ErrorMsg}", ex.Message);
            }
            return View(model);
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(PermissionViewModel model)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(model.RoleId);
                var existingClaims = await _roleManager.GetClaimsAsync(role);
                var selectedClaims = model.RoleClaims.Where(a => a.Selected).ToList();
                var newSelectedClaims = new List<RoleClaimsViewModel>();

                // Remove claims not present in selectedClaims
                foreach (var claimValue in existingClaims)
                {
                    if (!selectedClaims.Any(claim => claim.Value == claimValue.Value))
                    {
                        await _roleManager.RemoveClaimAsync(role, claimValue);
                    }
                }

                foreach (var claim in selectedClaims)
                {
                    var splitter = claim.Value.Split('.');
                    var getApp = splitter[1];
                    var getModule = splitter[2];
                    var viewClaimValue = "Permissions." + getApp + "." + getModule + ".View";

                    if (!selectedClaims.Any(c => c.Value == viewClaimValue))
                    {
                        var viewClaim = new RoleClaimsViewModel
                        {
                            Type = "Permission",
                            Value = viewClaimValue,
                            Selected = true
                        };
                        newSelectedClaims.Add(viewClaim);
                    }

                    newSelectedClaims.Add(claim);
                }

                foreach (var claim in newSelectedClaims)
                {
                    await _roleManager.AddPermissionClaim(role, claim.Value);
                }

                TempData["SuccessmsgPop"] = TANResource.SuccessMsg.ToString();
            }
            catch (Exception ex)
            {
                Log.Error("An Error Occurred", ex.Message);
                TempData["ErrormsgPop"] = TANResource.ErrorMsg.ToString();
            }

            return RedirectToAction("Index", new { roleId = model.RoleId });
        }

    }
}
