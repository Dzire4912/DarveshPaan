using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TANWeb.Resources;
using static TAN.DomainModels.Helpers.Permissions;
using System.Data;
using System.Security.Claims;
using TANWeb.Helpers;
using TANWeb.Controllers;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using System.Linq.Expressions;
using Serilog;
using Microsoft.AspNetCore.RateLimiting;
using System.Linq;

namespace TANWeb.Areas.TANUserManagement.Controllers
{
    [Area("TANUserManagement")]
    [Authorize(Roles = "SuperAdmin")]
    public class RolesController : BaseController
    {
        private readonly RoleManager<AspNetRoles> _roleManager;
        private IUnitOfWork _uow;
        public RolesController(IUnitOfWork uow, RoleManager<AspNetRoles> roleManager) : base(uow)
        {
            _roleManager = roleManager;
            _uow = uow;
        }
        [Authorize(Permissions.RoleManagement.PBJSnapView)]
        //[Route("PBJ/Roles")]
        [Route("/roles")]
        public async Task<IActionResult> Index(string appId, string roleId, string userType, string Id = "")
        {
            var roleName = "";
            RolesViewModel Model = new RolesViewModel();
            try
            {
                var role = await _roleManager.FindByIdAsync(Id);
                var list = new List<AspNetRoles>();

                if (role != null)
                {
                    roleName = role.Name;

                }
                if (Convert.ToInt32(userType) == Convert.ToInt32(UserTypes.Thinkanew))
                {
                    if (roleId == null)
                    {
                        list = _roleManager.Roles.Where(x => x.RoleType == RoleTypes.TANApplicationUser.ToString() && x.ApplicationId == "0" && x.NormalizedName != Roles.SuperAdmin.ToString().ToUpper()).ToList();
                    }
                    else
                    {
                        list = _roleManager.Roles.Where(x => x.RoleType == RoleTypes.TANApplicationUser.ToString() && x.ApplicationId == "0" && x.NormalizedName != Roles.SuperAdmin.ToString().ToUpper() && x.Id == roleId).ToList();
                    }
                }
                else
                {
                    if (appId == null && roleId == null)
                    {
                        list = _roleManager.Roles.Where(x => x.Name != Roles.SuperAdmin.ToString() && x.RoleType != RoleTypes.TANApplicationUser.ToString()).ToList();
                    }
                    else if (appId != null && roleId == null)
                    {
                        list = _roleManager.Roles.Where(x => x.Name != Roles.SuperAdmin.ToString() && x.RoleType != RoleTypes.TANApplicationUser.ToString() && x.ApplicationId == appId).ToList();
                    }
                    else if (appId == null && roleId != null)
                    {
                        list = _roleManager.Roles.Where(x => x.Name != Roles.SuperAdmin.ToString() && x.RoleType != RoleTypes.TANApplicationUser.ToString() && x.Id == roleId).ToList();
                    }
                    else if (appId != null && roleId != null)
                    {
                        list = _roleManager.Roles.Where(x => x.Name != Roles.SuperAdmin.ToString() && x.RoleType != RoleTypes.TANApplicationUser.ToString() && x.ApplicationId == appId && x.Id == roleId).ToList();
                    }
                }

                var appList = _uow.ApplicationRepo.GetAll().ToList();
                Model.RoleName = roleName;
                Model.ApplicationList = _uow.ApplicationRepo.GetAll().Select(p => new Itemlist
                {
                    Text = p.Name,
                    Value = p.ApplicationId
                }).ToList();
                var roleModel = new List<RoleView>();
                foreach (var item in list)
                {
                    RoleView roleView = new RoleView();
                    roleView.RoleId = item.Id;
                    roleView.RoleName = item.Name;
                    roleView.NormalizedName = item.NormalizedName;
                    roleView.ApplicationId = item.ApplicationId;

                    foreach (var app in appList)
                    {
                        if (item.ApplicationId == app.ApplicationId.ToString())
                        {
                            roleView.ApplicationName = app.Name;
                        }

                    }
                    roleModel.Add(roleView);

                }
                Model.RolesList = roleModel;

                Model.AppRoleList = _uow.AspNetRolesRepo.GetAll().Where(x => x.NormalizedName != Roles.SuperAdmin.ToString().ToUpper()).Select(s => new roleItemlist { Text = s.Name, Value = s.Id }).ToList();

                Model.UserTypesItems = _uow.UserTypeRepo.GetAll().Select(ut => new userTypeItemList { Text = ut.Name, Value = ut.Id.ToString() }).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Roles Controller :{ErrorMsg}", ex.Message);
            }
            return View(Model);
        }

        [Authorize(Permissions.RoleManagement.PBJSnapView)]

        [Route("Roles/GetRoleDetails")]
        public async Task<IActionResult> GetRoleDetails(string Id = "")
        {
            RolesViewModel Model = new RolesViewModel();
            try
            {
                var user = await _roleManager.FindByIdAsync(Id);
                string[] role;
                var roleName = string.Empty;
                if (user != null)
                {
                    roleName = user.Name;
                }
                Model.RoleName = roleName == string.Empty ? user.Name : roleName;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Roles Controller :{ErrorMsg}", ex.Message);
                return Json(new RolesViewModel());
            }
            return Json(Model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Permissions.RoleManagement.PBJSnapCreate)]
        [RateLimitForRole(requestCount:2,timeInMinutes:(int)TimeInMinutes.Minute)]
        public async Task<IActionResult> AddRole(RolesViewModel Model)
        {
            try
            {
                if (HttpContext.Items.TryGetValue(RateLimitForRoleAttribute.RateLimitExceededKey, out var rateLimitExceeded))
                {
                    if (rateLimitExceeded is bool && (bool)rateLimitExceeded)
                    {
                        TempData["ErrormsgPopup"] = "Too Many Requests, Please try after some time.";
                    }
                }
                else if (Model.RoleName != null && Model.RoleName != Roles.SuperAdmin.ToString())
                {
                    string name = Model.RoleName;
                    //  var role = await _roleManager.FindByNameAsync(name);

                    AspNetRoles aspNetRoles = new AspNetRoles();
                    aspNetRoles.Name = name.Trim();
                    aspNetRoles.ApplicationId = Model.ApplicationName;
                    IdentityResult identityResult = await _roleManager.CreateAsync(aspNetRoles);
                    if (identityResult != null)
                    {
                        if (identityResult.Succeeded)
                        {
                            TempData["SuccessmsgPop"] = TANResource.SuccessMsg.ToString();
                        }
                        foreach (var error in identityResult.Errors)
                        {
                            if (error.Code == IdentityRolesStatus.DuplicateRoleName.ToString())
                            {
                                TempData["WarningmsgPop"] = "RoleName is already exists!please try different role name.";
                            }
                            else
                            {
                                TempData["ErrormsgPopup"] = TANResource.ErrorMsg.ToString();
                            }
                        }
                    }
                }
                else
                {
                    TempData["ErrormsgPopup"] = "PLease enter role name";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Roles Controller :{ErrorMsg}", ex.Message);
                TempData["ErrormsgPopup"] = "Something went wrong";
            }
            return RedirectToAction("Index");
        }

        [Authorize(Permissions.RoleManagement.PBJSnapEdit)]
        public async Task<IActionResult> UpdateRole(string roleName, string Id)
        {
            var respMsg = string.Empty;
            if (roleName != null && roleName != Roles.SuperAdmin.ToString())
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                //if (role == null)
                //{
                var user = await _roleManager.FindByIdAsync(Id);
                user.Name = roleName;
                user.Id = Id;
                try
                {
                    IdentityResult result = await _roleManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        respMsg = TANResource.SuccessMsg.ToString();
                        TempData["SuccessmsgPop"] = TANResource.SuccessMsg.ToString();
                    }
                    else if (result.Errors.Count() > 0)
                    {
                        foreach (var error in result.Errors)
                        {
                            if (error.Code == IdentityRolesStatus.DuplicateRoleName.ToString())
                            {
                                respMsg = "RoleName is already exists!please try different role name.";
                                TempData["WarningmsgPop"] = respMsg;
                            }
                            else
                            {
                                respMsg = TANResource.ErrorMsg.ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An Error Occured in Roles Controller :{ErrorMsg}", ex.Message);
                }
            }
            return Json(respMsg);
        }

        public async Task<IActionResult> DeleteRole(string Id)
        {
            var respMsg = string.Empty;
            try
            {
                var user = await _roleManager.FindByIdAsync(Id);
                if (user != null)
                {
                    user.Id = Id;
                    IdentityResult result = await _roleManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        respMsg = TANResource.DeleteMsg.ToString();
                        TempData["SuccessmsgPop"] = TANResource.DeleteMsg.ToString();
                    }
                    else
                    {
                        respMsg = TANResource.ErrorMsg.ToString();
                    }
                }
                else
                {
                    respMsg = TANResource.ErrorMsg.ToString();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Roles Controller / Delete Role :{ErrorMsg}", ex.Message);
                respMsg = TANResource.ErrorMsg.ToString();
                return Json(respMsg);
            }
            return Json(respMsg);
        }

        public JsonResult GetTanRoles()
        {
            try
            {
                var roleList = _uow.AspNetRolesRepo.GetAll().Where(x => x.RoleType == RoleTypes.TANApplicationUser.ToString() && x.NormalizedName != Roles.SuperAdmin.ToString().ToUpper()).Select(x => new roleItemlist { Text = x.Name, Value = x.Id.ToString() }).ToList();
                return Json(roleList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Roles Controller / Getting TAN Roles :{ErrorMsg}", ex.Message);
                return Json(new AspNetRoles());
            }
        }

    }
}

