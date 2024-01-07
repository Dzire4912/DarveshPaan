using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TANWeb.Controllers;
using TANWeb.Resources;

namespace TANWeb.Areas.TANUserManagement.Controllers
{
    [Area("TANUserManagement")]
    [Authorize]
    public class UserRolesController : BaseController
    {
        private readonly SignInManager<AspNetUser> _signInManager;
        private readonly UserManager<AspNetUser> _userManager;
        private readonly RoleManager<AspNetRoles> _roleManager;
        private IUnitOfWork _uow;
        private readonly IMemoryCache _cache;
        public UserRolesController(IUnitOfWork uow, UserManager<AspNetUser> userManager, SignInManager<AspNetUser> signInManager, RoleManager<AspNetRoles> roleManager, IMemoryCache memoryCache) : base(uow)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _uow = uow;
            _cache = memoryCache;
        }
        //gets the role and rolelist againgst user
        public async Task<IActionResult> Index(string userId, string? roleId, string? appId, string? userType)
        {
            var viewModel = new List<UserRolesViewModel>();
            var model = new ManageUserRolesViewModel();

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                var userApplication = _uow.UserApplicationRepo.GetAll().Where(x => x.UserId == userId).Select(x => x.ApplicationId.ToString()).ToList();
                var aspNetRoles = new List<AspNetRoles>();
                model.UserId = userId;
                model.UserRoles = viewModel;
                userType = (Convert.ToInt32(UserTypes.Thinkanew) == user.UserType) ? UserTypes.Thinkanew.ToString() : UserTypes.Other.ToString();

                var userApplicationIds = _uow.UserApplicationRepo.GetAll()
                                         .Where(x => x.UserId == userId)
                                         .Select(x => x.ApplicationId)
                                         .ToList();

                // Rest of your code...
                var filteredRoles = _roleManager.Roles
                    .Where(role =>
                        (userType == UserTypes.Thinkanew.ToString() && (roleId == null || role.Id == roleId) &&
                            role.RoleType == RoleTypes.TANApplicationUser.ToString() &&
                            role.NormalizedName != Roles.SuperAdmin.ToString().ToUpper()) ||
                        (userType == UserTypes.Other.ToString() &&
                            (appId == null || role.ApplicationId == appId) &&
                            (roleId == null || role.Id == roleId) &&
                            role.NormalizedName != Roles.SuperAdmin.ToString().ToUpper() &&
                            role.RoleType != RoleTypes.TANApplicationUser.ToString()) ||
                        (userType != UserTypes.Thinkanew.ToString() && userType != UserTypes.Other.ToString() &&
                            userApplicationIds.Contains(Convert.ToInt32(role.ApplicationId)) &&
                            role.NormalizedName != Roles.SuperAdmin.ToString().ToUpper())
                    ).ToList();

                aspNetRoles.AddRange(filteredRoles);

                foreach (var role in aspNetRoles)
                {
                    var userRolesViewModel = new UserRolesViewModel
                    {
                        RoleName = role.Name,
                        RoleId = role.Id,
                        AppId = role.ApplicationId,
                        Selected = model.UserRoles.Any(x => x.RoleId == role.Id),
                        AppName = _uow.ApplicationRepo.GetAll().FirstOrDefault(x => x.ApplicationId.ToString() == role.ApplicationId)?.Name
                    };

                    viewModel.Add(userRolesViewModel);
                }

                if (appId != null && roleId != null)
                {
                    model.UserRoles = viewModel.Where(x => x.RoleId == roleId && x.AppId == appId).ToList();
                }
                else if (appId != null && roleId == null)
                {
                    model.UserRoles = viewModel.Where(x => x.AppId == appId).ToList();
                }
                else if (appId == null && roleId != null)
                {
                    model.UserRoles = viewModel.Where(x => x.RoleId == roleId).ToList();
                }
                if (user.UserType != Convert.ToInt32(UserTypes.Thinkanew))
                {
                    model.AppItems = _uow.ApplicationRepo.GetAll().Select(_ => new appItemlist { Text = _.Name, Value = _.ApplicationId }).ToList();
                }
                else
                {
                    model.AppItems = new List<appItemlist>();
                }
                model.UserTypesItems = _uow.UserTypeRepo.GetAll().Select(ut => new userTypeItemList { Text = ut.Name, Value = ut.Id.ToString() }).ToList();
                model.RoleItems = viewModel.Select(item => new roleItemlist { Text = item.RoleName, Value = item.RoleId }).ToList();
                model.UserName = $"{user.FirstName} {user.LastName}";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occurred in User Roles Controller: {ErrorMsg}", ex.Message);
            }

            return View(model);
        }


        public async Task<IActionResult> Update(string id, ManageUserRolesViewModel model)
        {
            try
            {
                using (var transaction = _uow.BeginTransaction()) // Assuming _uow supports transactions
                {
                    var user = await _userManager.FindByIdAsync(id);
                    var deleteItems = _uow.AspNetUserRoleRepo.GetAll().Where(x => x.UserId == model.UserId).ToList();

                    _uow.AspNetUserRoleRepo.RemoveRange(deleteItems);
                    _uow.SaveChanges();

                    var neUsrRoles = model.UserRoles.Where(x => x.Selected).ToList();
                    var UserApp = _uow.UserApplicationRepo.GetAll().Where(x => x.UserId == model.UserId).ToList();

                    foreach (var role in neUsrRoles)
                    {
                        try
                        {
                            var aspNetUserRoles = new AspNetUserRoles
                            {
                                RoleId = role.RoleId,
                                UserId = model.UserId
                            };

                            _uow.AspNetUserRoleRepo.Add(aspNetUserRoles);

                            var userApplication = new UserApplication
                            {
                                ApplicationId = Convert.ToInt32(role.AppId),
                                UserId = model.UserId
                            };

                            if (userApplication.ApplicationId != 0 && !_uow.UserApplicationRepo.GetAll().Any(x => x.ApplicationId == userApplication.ApplicationId && x.UserId == userApplication.UserId))
                            {
                                _uow.UserApplicationRepo.Add(userApplication);
                            }
                            _uow.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            Log.Error("An Error Occurred", ex.Message);
                            TempData["ErrormsgPopup"] = TANResource.ErrorMsg.ToString();
                            transaction.Rollback();
                            //log
                        }
                    }
                    transaction.Commit();
                    var currentUser = await _userManager.GetUserAsync(User);
                    _cache.Remove("UserRoles" + user.Id);
                    await _signInManager.RefreshSignInAsync(currentUser);
                    TempData["SuccessmsgPop"] = TANResource.SuccessMsg.ToString();

                    // Commit the transaction
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occurred in User Roles Controller / Updating User Roles: {ErrorMsg}", ex.Message);
            }

            return RedirectToAction("UserList", "User", new { Area = "TANUserManagement" });
        }

        public JsonResult GetTanRoles()
        {
            try
            {
                var appList = _uow.AspNetRolesRepo.GetAll().Where(x => x.RoleType == RoleTypes.TANApplicationUser.ToString() && x.NormalizedName != Roles.SuperAdmin.ToString().ToUpper()).Select(x => new roleItemlist { Text = x.Name, Value = x.Id.ToString() }).ToList();
                return Json(appList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in User Roles Controller / Getting TAN Roles :{ErrorMsg}", ex.Message);
                return Json(new AspNetRoles());
            }
        }
    }
}