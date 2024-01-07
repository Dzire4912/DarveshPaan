using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Serilog;
using System.Data;
using System.Security.Claims;
using System.Text;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TANWeb.Helpers;
using TANWeb.Models;
using TANWeb.Resources;
using Utils = TANWeb.Helpers.Utils;

namespace TANWeb.Areas.TANUserManagement.Controllers
{
    [Area("TANUserManagement")]
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserManager<AspNetUser> _userManager;
        private readonly RoleManager<AspNetRoles> _roleManager;
        private IUnitOfWork _uow;
        private readonly IUserStore<AspNetUser> _userStore;
        private readonly IUserEmailStore<AspNetUser> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly IEmailHelper _emailHelper;
        private readonly IMailContentHelper _mailContentHelper;
        private readonly IConfiguration _configuration;

        public UserController(IUnitOfWork uow, UserManager<AspNetUser> userManager, RoleManager<AspNetRoles> roleManager, IEmailSender emailSender, IMailContentHelper mailContentHelper, IConfiguration configuration, IEmailHelper emailHelper)
        {
            _uow = uow;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _mailContentHelper = mailContentHelper;
            _configuration = configuration;
            _emailHelper = emailHelper;
        }

        //[Route("PBJ/User/UserList")]
        [Route("/users")]
        public async Task<IActionResult> UserList(string? userName, string? role, string? fOrgId, string? fFacilityId)
        {
            UserViewModel userViewModel = new UserViewModel();
            try
            {
                var allUsers = new List<User>();
                var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                var appId = string.Empty;
                int OrgId = 0;
                //Check Cuurent App
                string sessionValue = HttpContext.Session.GetString("SelectedApp");
                if (sessionValue != null)
                {
                    var app = _uow.ApplicationRepo.GetAll().Where(x => x.Name == sessionValue && x.IsActive).FirstOrDefault();

                    if (app != null)
                    {
                        appId = app.ApplicationId.ToString();
                    }
                }
                //Get Users
                if (currentUser != null && currentUser.UserType == Convert.ToInt32(UserTypes.Thinkanew))
                {
                    allUsers = await _uow.EmployeeRepo.listUsers(currentUser.Id, appId, OrgId);
                }
                else
                {
                    var cOrg = _uow.UserOrganizationFacilitiesRepo.GetAll().FirstOrDefault(x => x.UserId == currentUser.Id);
                    OrgId = cOrg.OrganizationID;
                    ViewBag.currentUserOrg = cOrg.OrganizationID;
                    allUsers = await _uow.EmployeeRepo.listUsers(currentUser.Id, appId, OrgId);
                }
                userViewModel.users = allUsers;
                #region Filters
                //FilterByUsername
                if (!string.IsNullOrEmpty(userName))
                {
                    if (userName.Contains(' '))
                    {
                        var splittedUserName = userName.Split(' ');
                        var firstName = splittedUserName[0];
                        var lastName = splittedUserName[1];
                        try
                        {
                            allUsers = allUsers.Where(a => a.UserName == userName || (a.FirstName.Contains(firstName) || a.LastName.Contains(lastName))).ToList();
                            userViewModel.users = allUsers;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
                        }
                    }
                    else
                    {
                        try
                        {
                            allUsers = allUsers.Where(a => (a.UserName.Contains(userName) || a.FirstName.Contains(userName) || a.LastName.Contains(userName))).ToList();
                            userViewModel.users = allUsers;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
                        }
                    }
                }
                //FilterByRole
                if (role != null)
                {
                    allUsers = allUsers.Where(x => x.Role == role).ToList();
                    userViewModel.users = allUsers;
                }
                //filter by Org
                if (fOrgId != null)
                {
                    allUsers = allUsers.Where(x => x.OrgId.Equals(Convert.ToInt32(fOrgId))).ToList();
                    userViewModel.users = allUsers;
                }
                if (fFacilityId != null)
                {
                    allUsers = allUsers.Where(x => x.FacilityId.Equals(Convert.ToInt32(fFacilityId))).ToList();
                    userViewModel.users = allUsers;
                }
                #endregion

                userViewModel.ApplicationList = _uow.ApplicationRepo.GetAll().Where(p => p.IsActive).Select(p => new Itemlist
                {
                    Text = p.Name,
                    Value = p.ApplicationId
                }).ToList();
                //get list of roles except super Admin
                userViewModel.RoleList = _roleManager.Roles.Where(s => s.NormalizedName != Roles.SuperAdmin.ToString().ToUpper() &&(s.ApplicationId==appId || s.RoleType==RoleTypes.TANApplicationUser.ToString())).Select(p => new roleItemlist
                {
                    Text = p.Name, 
                    Value = p.Id
                }).ToList();
                //roles for tan user
                userViewModel.PopUpRoles = _roleManager.Roles.Where(s => s.NormalizedName != Roles.SuperAdmin.ToString().ToUpper() && s.RoleType == RoleTypes.TANApplicationUser.ToString()).Select(p => new roleItemlist
                {
                    Text = p.Name,
                    Value = p.Id
                }).ToList();
                userViewModel.OrgList = _uow.OrganizationRepo.GetAll().Where(p => p.IsActive).Select(x => new Itemlist { Text = x.OrganizationName, Value = x.OrganizationID }).ToList();
                userViewModel.UserTypesList = _uow.UserTypeRepo.GetAll().Select(x => new Itemlist { Text = x.Name, Value = x.Id }).ToList();
                userViewModel.FacilityList = _uow.FacilityRepo.GetAll().Select(x => new Itemlist { Text = x.FacilityName, Value = x.Id }).ToList();

                ViewBag.currentUserType = currentUser.UserType;

                if (TempData.TryGetValue("AddUserId", out var AddUserId1))
                {
                    ViewBag.AddUserId = AddUserId1;
                }
                if (TempData.TryGetValue("fromFacility", out var fromFacility1))
                {
                    ViewBag.fromFacility = fromFacility1;
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in User Controller :{ErrorMsg}", ex.Message);
            }
            return View("~/Areas/TANUserManagement/Views/User/UserList.cshtml", userViewModel);
        }

        [Route("PBJ/Users/UpdateActiveStatus/{id?}")]
        public async Task<IActionResult> UpdateActiveStatus(string userId, string status)
        {

            string response = "";
            var userStatus = true;
            try
            {
                if (status == "0")
                {
                    userStatus = false;
                }
                //change status
                if (userId != null && status != null)
                {
                    var employeeList = _userManager.Users.Where(x => x.Id.Equals(userId)).FirstOrDefault();
                    if (employeeList != null)
                    {
                        employeeList.IsActive = Convert.ToBoolean(userStatus);
                        await _userManager.UpdateAsync(employeeList);
                        response = "success";
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in User Controller :{ErrorMsg}", ex.Message);
            }
            return Json(response);
        }


        [Authorize(Permissions.UserManagement.PBJSnapCreate)]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [EnableRateLimiting("AddPolicy")]
        public async Task<IActionResult> AddTANUserInfo(string fName, string lName, string email, string UserType, string[] apps, string defaultApp, string defApp, string roleName, string? cuType, string? cuOrg)
        {
            string returnUrl = Url.Content("~/");
            string response = string.Empty;
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            if (currentUser != null && currentUser.UserType == Convert.ToInt32(UserTypes.Other))
            {
                UserType = cuType;
                defaultApp = cuOrg;
            }
            using (var transaction = _uow.BeginTransaction())
            {
                try
                {
                    var newUser = new AspNetUser();
                    newUser.UserName = email;
                    newUser.Email = email;
                    newUser.EmailConfirmed = false;
                    newUser.NormalizedUserName = email;
                    newUser.PhoneNumberConfirmed = true;
                    newUser.UserType = Convert.ToInt32(UserType); // need to remove
                    newUser.IsActive = true;
                    newUser.TwoFactorEnabled = true;
                    newUser.DefaultAppId = Convert.ToInt32(defApp);
                    newUser.FirstName = fName;
                    newUser.LastName = lName;

                    var user = await _userManager.FindByEmailAsync(email);
                    if (user == null)
                    {
                        Utils utils = new Utils();

                        string password = utils.CreateDummyPassword();
                        newUser.PhoneNumber = newUser.PhoneNumber;
                        newUser.CreatedBy = currentUser.Id;
                        var result = await _userManager.CreateAsync(newUser, password);
                        await _userManager.AddClaimAsync(newUser, new Claim("UserName", (newUser.FirstName + " " + newUser.LastName)));
                        //await _userManager.AddClaimAsync(newUser, new Claim("Permission", Permissions.UserManagement.PBJSnapEdit));
                        if (result.Succeeded)
                        {

                            var facilityAppList = new List<FacilityApplicationModel>();
                            if (newUser.UserType != Convert.ToInt32(UserTypes.Thinkanew))
                            {
                                response = await saveUserInfoInUserOranizationFacility(newUser, defaultApp, apps, response);
                            }
                            else
                            {
                                try
                                {
                                    AspNetUserRoles aspNetUserRoles = new();
                                    aspNetUserRoles.RoleId = roleName;
                                    aspNetUserRoles.UserId = newUser.Id;
                                    _uow.AspNetUserRoleRepo.Add(aspNetUserRoles);
                                    _uow.SaveChanges();
                                    var allApps = _uow.ApplicationRepo.GetAll();
                                    foreach (var item in allApps)
                                    {
                                        UserApplication userApplication = new UserApplication();
                                        userApplication.ApplicationId = Convert.ToInt32(item.ApplicationId);
                                        userApplication.UserId = newUser.Id;
                                        _uow.UserApplicationRepo.Add(userApplication);
                                    }
                                    _uow.SaveChanges();
                                    response = "success";
                                }
                                catch (Exception ex)
                                {
                                    Log.Error(ex, "An Error Occured in User Controller / Add ThinkAnew User Role :{ErrorMsg}", ex.Message);
                                    response = "";
                                }
                            }
                            if (string.IsNullOrEmpty(response))
                            {
                                response = "";
                                transaction.Rollback();
                                Log.Error(response, "An Error Occured in User Controller  :{ErrorMsg}", "Response is NULL");
                            }
                            else
                            {
                                var userId = await _userManager.GetUserIdAsync(newUser);
                                var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                                List<string> recipients = new List<string> { newUser.Email };
                                SendMail sendMail = new();
                                EmailTemplateViewModel emailTemplateVM = new();
                                emailTemplateVM.DummyPassword = password;
                                emailTemplateVM.MailFormat = "Password";
                                emailTemplateVM.TemplateFilePath = TANResource.ResetPasswordEmailTemplatePath;
                                emailTemplateVM.Message = "Password.";
                                emailTemplateVM.UserName = newUser.FirstName;
                                string server = _configuration.GetValue<string>("DomainPath:Server");
                                string logoPath = _configuration.GetValue<string>("DomainPath:LogoPath");
                                emailTemplateVM.LogoPath = server + logoPath + "logo1.png";
                                emailTemplateVM.ForgotPasswordLogoPath = server + logoPath + "forgot-the-pass.png";
                                emailTemplateVM.Url = server;
                                sendMail.Recipients = recipients.ToArray();
                                sendMail.Subject = "Think Anew Email Confirmation";
                                sendMail.Body = await _mailContentHelper.CreateMailContent(emailTemplateVM);
                               bool isMailSent= await _emailHelper.SendEmailAsync(sendMail);
                                if (isMailSent)
                                {
                                    transaction.Commit();
                                    response = "success";
                                    TempData["SuccessmsgPop"] = TANResource.SuccessMsg.ToString();
                                }
                                else
                                {
                                    response = "";
                                    transaction.Rollback();
                                    Log.Error("Add User Failed", "An Error Occured in while sending email in User Controller  :{ErrorMsg}", result.Errors);
                                }
                            }
                        }
                        else
                        {
                            response = "";
                            transaction.Rollback();
                            Log.Error("Add User Failed", "An Error Occured in User Controller  :{ErrorMsg}", result.Errors);
                        }
                    }
                    else
                    {
                        response = "exist";
                        transaction.Rollback();
                        Log.Error(response, "An Error Occured in User Controller  :{ErrorMsg}", "User already exists");
                    }
                }
                catch (Exception ex)
                {
                    response = "";
                    transaction.Rollback();
                    Log.Error(ex, "An Error Occurred in User Controller/Add User:{ErrorMsg}", ex.Message);
                }
            }
            return Json(response);
        }

        private async Task<string> saveUserInfoInUserOranizationFacility(AspNetUser newUser, string? organization, string[] facilityList, string? response)
        {
            try
            {
                var existingUserOrganizationFacilities = _uow.UserOrganizationFacilitiesRepo.GetAll().Where(x => x.UserId == newUser.Id).ToList();
                if (existingUserOrganizationFacilities.Count.Equals(0))
                {
                    foreach (var facility in facilityList)
                    {
                        UserOrganizationFacility userOrganizationFacility = new();
                        userOrganizationFacility.UserId = newUser.Id;
                        userOrganizationFacility.OrganizationID = Convert.ToInt32(organization);
                        userOrganizationFacility.FacilityID = Convert.ToInt32(facility);
                        userOrganizationFacility.CreatedBy = newUser.CreatedBy;
                        userOrganizationFacility.CreatedDate = DateTime.UtcNow;
                        userOrganizationFacility.IsActive = true;

                        _uow.UserOrganizationFacilitiesRepo.Add(userOrganizationFacility);
                        //_uow.SaveChanges();

                        var facilityApplications = _uow.FacilityApplicationRepo.GetAll().Where(f => f.FacilityId == Convert.ToInt32(facility) && f.IsActive).ToList();
                        foreach (var facilityApplication in facilityApplications)
                        {
                            var existingapp = _uow.UserApplicationRepo.GetAll().Where(app => app.UserId == newUser.Id && app.ApplicationId == facilityApplication.ApplicationId).FirstOrDefault();
                            if (existingapp == null)
                            {
                                UserApplication userApplication = new();
                                userApplication.UserId = newUser.Id;
                                userApplication.ApplicationId = facilityApplication.ApplicationId;
                                _uow.UserApplicationRepo.Add(userApplication);
                            }
                        }
                    }
                    _uow.SaveChanges();
                    response = "success";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in User Controller/Save User Organization & Facilities:{ErrorMsg}", ex.Message);
                response = "";
            }
            return response;
        }

        [HttpGet]
        [Route("PBJ/Users/EditUser/{id?}")]
        [Authorize(Permissions.UserManagement.PBJSnapEdit)]
        public async Task<ActionResult> EditUser(string userId)
        {
            EditUserViewModel userViewModel = new EditUserViewModel();
            try
            {
                var user = _userManager.Users.Where(x => x.Id.Equals(userId)).FirstOrDefault();
                var userApplication = _uow.UserApplicationRepo.GetAll().Where(x => x.UserId.Equals(userId)).ToList();
                List<string> selectedApps = new List<string>();
                List<string> sApps = new List<string>();
                //get list apps allowed 
                if (userApplication != null)
                {
                    foreach (var item in userApplication)
                    {
                        sApps.Add(item.ApplicationId.ToString());
                    }
                    foreach (string sApp in sApps)
                    {
                        selectedApps.Add(sApp);
                    }
                }
                //get role list
                userViewModel.RoleList = _roleManager.Roles.Where(s => s.Name != Roles.SuperAdmin.ToString()).Select(p => new roleItemlist
                {
                    Text = p.Name,
                    Value = p.Id
                }).ToList();
                //get app list
                userViewModel.ApplicationList = _uow.ApplicationRepo.GetAll().Select(p => new appItemlist
                {
                    Text = p.Name,
                    Value = p.ApplicationId
                }).ToList();
                var applist = new List<appItemlist>();
                foreach (var item in userViewModel.ApplicationList)
                {
                    try
                    {
                        if (selectedApps.Contains(item.Value.ToString()))
                        {
                            var app = _uow.ApplicationRepo.GetAll().Where(x => x.ApplicationId == Convert.ToInt32(item.Value)).Select(p => new appItemlist { Text = p.Name, Value = p.ApplicationId, Selected = false }).FirstOrDefault();
                            applist.Add(app);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "An Error Occured in User Controller/Edit User Information:{ErrorMsg}", ex.Message);
                    }
                }
                userViewModel.ApplicationList = applist;

                //allowed apps are marked selected
                foreach (var option in userViewModel.ApplicationList)
                {
                    if (selectedApps.Contains(option.Value.ToString()))
                    {
                        option.Selected = true;
                    }
                }
                //assigning user info
                AspNetUser editUser = new AspNetUser()
                {
                    Email = user.Email,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    DefaultAppId = user.DefaultAppId,
                    Id = user.Id,
                    UserType = user.UserType
                };
                userViewModel.users = editUser;

                if (editUser.UserType != Convert.ToInt32(UserTypes.Thinkanew))
                {
                    var userOrganizationFacilitiesInfo = _uow.UserOrganizationFacilitiesRepo.GetAll().Where(x => x.UserId == userId).ToList();
                    var apps = new List<appItemlist>();
                    applist.Clear();
                    userViewModel.FacilityList = new List<facilityItemList>();
                    userViewModel.OrgList = new List<appItemlist>();
                    facilityItemList userFacilityInfo = new facilityItemList();
                    foreach (var userfacility in userOrganizationFacilitiesInfo)
                    {
                        var userOrganization = _uow.OrganizationRepo.GetAll().Where(x => x.OrganizationID == userfacility.OrganizationID && x.IsActive).Select(p => new appItemlist { Text = p.OrganizationName, Value = p.OrganizationID, Selected = false }).FirstOrDefault();
                        if (userOrganization != null)
                        {
                            userFacilityInfo = _uow.FacilityRepo.GetAll().Where(x => x.OrganizationId == Convert.ToInt32(userOrganization.Value) && x.Id == userfacility.FacilityID && x.IsActive).Select(p => new facilityItemList { Text = p.FacilityName, Value = p.Id.ToString(), Selected = false }).FirstOrDefault();
                        }
                        #region commented
                        //if (userFacility.Contains(','))
                        //{
                        //    string[] splitresult = userFacility.Split(',');

                        //    foreach (string item in splitresult)
                        //    {
                        //        var userFacilityList = _uow.FacilityRepo.GetAll().Where(x => x.FacilityID == Convert.ToInt32(item)).Select(p => new facilityItemList { Text = p.FacilityName, Value = p.FacilityID.ToString() }).FirstOrDefault();
                        //        asignfacility.Add(userFacilityList);
                        //    }
                        //    userViewModel.FacilityList= asignfacility;
                        //}
                        //else
                        //{
                        //    var userFacilityList = _uow.FacilityRepo.GetAll().Where(x => x.FacilityID == Convert.ToInt32(userFacility[0])).Select(p => new facilityItemList { Text = p.FacilityName, Value = p.FacilityID.ToString() }).FirstOrDefault();
                        //    asignfacility.Add(userFacilityList);
                        //    userViewModel.FacilityList= asignfacility;
                        //}
                        #endregion
                        if (userFacilityInfo.Text != null)
                        {
                            userViewModel.FacilityList.Add(userFacilityInfo);
                        }
                        if (userOrganization != null)
                        {
                            if (!userViewModel.OrgList.Any(a => a.Value == userOrganization.Value))
                            {
                                userViewModel.OrgList.Add(userOrganization);
                            }
                        }
                    }
                    var organizationFacilites = new List<facilityItemList>();
                    if (userViewModel.OrgList.Count > 0)
                    {
                        organizationFacilites = _uow.FacilityRepo.GetAll().Where(of => of.OrganizationId == Convert.ToInt32(userViewModel.OrgList[0].Value) && of.IsActive).Select(p => new facilityItemList { Text = p.FacilityName, Value = p.Id.ToString(), Selected = false }).ToList();
                    }
                    foreach (var organizationFacility in organizationFacilites)
                    {
                        if (!userViewModel.FacilityList.Any(f => f.Value == organizationFacility.Value))
                        {
                            userViewModel.FacilityList.Add(organizationFacility);
                        }
                    }
                    foreach (var facility in userViewModel.FacilityList)
                    {
                        if (userOrganizationFacilitiesInfo.Any(a => a.FacilityID == Convert.ToInt32(facility.Value) && a.IsActive))
                        {
                            if (userViewModel.FacilityList.Any(f => f.Value == facility.Value))
                            {
                                facility.Selected = true;
                            }
                            else
                            {
                                facility.Selected = false;
                            }

                            apps = _uow.FacilityApplicationRepo.GetAll().Where(f => f.FacilityId == Convert.ToInt32(facility.Value) && f.IsActive).Select(p => new appItemlist { Text = p.ApplicationId.ToString(), Value = Convert.ToInt32(p.ApplicationId) }).ToList();


                            foreach (var app in apps)
                            {
                                var applicationinfo = _uow.ApplicationRepo.GetAll().Where(a => a.ApplicationId == app.Value).Select(x => new appItemlist { Text = x.Name, Value = x.ApplicationId, Selected = true }).FirstOrDefault();
                                if (!applist.Any(a => a.Value == applicationinfo.Value))
                                {
                                    applist.Add(applicationinfo);
                                }
                            }
                        }
                        else
                        {
                            facility.Selected = false;
                        }
                    }
                    userViewModel.ApplicationList = applist;

                    foreach (var option in userViewModel.ApplicationList)
                    {
                        if (selectedApps.Contains(option.Value.ToString()))
                        {
                            option.Selected = true;
                        }
                    }
                    var OrgList = _uow.OrganizationRepo.GetAll().Select(x => new appItemlist { Text = x.OrganizationName, Value = x.OrganizationID, Selected = false }).ToList();
                    foreach (var org in OrgList)
                    {
                        if (userViewModel.OrgList.Any(o => o.Value == org.Value)) { org.Selected = true; }
                    }
                    userViewModel.OrgList = OrgList;
                }
                var userTypeList = _uow.UserTypeRepo.GetAll().Select(x => new appItemlist { Text = x.Name, Value = x.Id, Selected = false }).ToList();
                foreach (var typeitem in userTypeList)
                {
                    if (typeitem.Value == editUser.UserType)
                    {
                        typeitem.Selected = true;
                    }
                }
                userViewModel.UserTypeList = userTypeList;
                ViewBag.userType = editUser.UserType;
                var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                if (currentUser.Id != userId)
                {
                    ViewBag.editingAnotherUser = true;
                }
                else
                {
                    ViewBag.editingAnotherUser = false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in User Controller/Edit User Information:{ErrorMsg}", ex.Message);
            }
            return View("~/Areas/TANUserManagement/Views/User/EditUser.cshtml", userViewModel);
        }

        [Route("/myprofile")]
        public async Task<ActionResult> MyProfile()
        {
            EditUserViewModel userViewModel = new EditUserViewModel();
            try
            {
                var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                var user = _userManager.Users.Where(x => x.Id.Equals(currentUser.Id)).FirstOrDefault();
                var userApplication = _uow.UserApplicationRepo.GetAll().Where(x => x.UserId.Equals(currentUser.Id)).ToList();
                List<string> selectedApps = new List<string>();
                List<string> sApps = new List<string>();
                //get list apps allowed 
                if (userApplication != null)
                {
                    foreach (var item in userApplication)
                    {
                        sApps.Add(item.ApplicationId.ToString());
                    }
                    foreach (string sApp in sApps)
                    {
                        selectedApps.Add(sApp);
                    }
                }
                //get role list
                userViewModel.RoleList = _roleManager.Roles.Where(s => s.Name != Roles.SuperAdmin.ToString()).Select(p => new roleItemlist
                {
                    Text = p.Name,
                    Value = p.Id
                }).ToList();
                //get app list
                userViewModel.ApplicationList = _uow.ApplicationRepo.GetAll().Select(p => new appItemlist
                {
                    Text = p.Name,
                    Value = p.ApplicationId
                }).ToList();
                var applist = new List<appItemlist>();
                foreach (var item in userViewModel.ApplicationList)
                {
                    try
                    {
                        if (selectedApps.Contains(item.Value.ToString()))
                        {
                            var app = _uow.ApplicationRepo.GetAll().Where(x => x.ApplicationId == Convert.ToInt32(item.Value)).Select(p => new appItemlist { Text = p.Name, Value = p.ApplicationId, Selected = false }).FirstOrDefault();
                            applist.Add(app);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "An Error Occured in User Controller/My Profile User Information:{ErrorMsg}", ex.Message);
                    }
                }
                userViewModel.ApplicationList = applist;

                //allowed apps are marked selected
                foreach (var option in userViewModel.ApplicationList)
                {
                    if (selectedApps.Contains(option.Value.ToString()))
                    {
                        option.Selected = true;
                    }
                }
                //assigning user info
                AspNetUser editUser = new AspNetUser()
                {
                    Email = user.Email,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    DefaultAppId = user.DefaultAppId,
                    Id = user.Id,
                    UserType = user.UserType
                };
                userViewModel.users = editUser;

                if (editUser.UserType != Convert.ToInt32(UserTypes.Thinkanew))
                {
                    var userOrganizationFacilitiesInfo = _uow.UserOrganizationFacilitiesRepo.GetAll().Where(x => x.UserId.Equals(currentUser.Id)).ToList();
                    var apps = new List<appItemlist>();
                    applist.Clear();
                    userViewModel.FacilityList = new List<facilityItemList>();
                    userViewModel.OrgList = new List<appItemlist>();
                    foreach (var userfacility in userOrganizationFacilitiesInfo)
                    {
                        var userOrganization = _uow.OrganizationRepo.GetAll().Where(x => x.OrganizationID == userfacility.OrganizationID).Select(p => new appItemlist { Text = p.OrganizationName, Value = p.OrganizationID, Selected = false }).FirstOrDefault();
                        var userFacilityInfo = _uow.FacilityRepo.GetAll().Where(x => x.OrganizationId == Convert.ToInt32(userOrganization.Value) && x.Id == userfacility.FacilityID).Select(p => new facilityItemList { Text = p.FacilityName, Value = p.Id.ToString(), Selected = false }).FirstOrDefault();

                        #region commented
                        //if (userFacility.Contains(','))
                        //{
                        //    string[] splitresult = userFacility.Split(',');

                        //    foreach (string item in splitresult)
                        //    {
                        //        var userFacilityList = _uow.FacilityRepo.GetAll().Where(x => x.FacilityID == Convert.ToInt32(item)).Select(p => new facilityItemList { Text = p.FacilityName, Value = p.FacilityID.ToString() }).FirstOrDefault();
                        //        asignfacility.Add(userFacilityList);
                        //    }
                        //    userViewModel.FacilityList= asignfacility;
                        //}
                        //else
                        //{
                        //    var userFacilityList = _uow.FacilityRepo.GetAll().Where(x => x.FacilityID == Convert.ToInt32(userFacility[0])).Select(p => new facilityItemList { Text = p.FacilityName, Value = p.FacilityID.ToString() }).FirstOrDefault();
                        //    asignfacility.Add(userFacilityList);
                        //    userViewModel.FacilityList= asignfacility;
                        //}
                        #endregion
                        userViewModel.FacilityList.Add(userFacilityInfo);
                        if (!userViewModel.OrgList.Any(a => a.Value == userOrganization.Value))
                        {
                            userViewModel.OrgList.Add(userOrganization);
                        }
                    }
                    var organizationFacilites = _uow.FacilityRepo.GetAll().Where(of => of.OrganizationId == Convert.ToInt32(userViewModel.OrgList[0].Value)).Select(p => new facilityItemList { Text = p.FacilityName, Value = p.Id.ToString(), Selected = false }).ToList();
                    foreach (var organizationFacility in organizationFacilites)
                    {
                        if (!userViewModel.FacilityList.Any(f => f.Value == organizationFacility.Value))
                        {
                            userViewModel.FacilityList.Add(organizationFacility);
                        }
                    }
                    foreach (var facility in userViewModel.FacilityList)
                    {
                        if (userOrganizationFacilitiesInfo.Any(a => a.FacilityID == Convert.ToInt32(facility.Value) && a.IsActive))
                        {
                            if (userViewModel.FacilityList.Any(f => f.Value == facility.Value))
                            {
                                facility.Selected = true;
                            }
                            else
                            {
                                facility.Selected = false;
                            }

                            apps = _uow.FacilityApplicationRepo.GetAll().Where(f => f.FacilityId == Convert.ToInt32(facility.Value) && f.IsActive).Select(p => new appItemlist { Text = p.ApplicationId.ToString(), Value = Convert.ToInt32(p.ApplicationId) }).ToList();


                            foreach (var app in apps)
                            {
                                var applicationinfo = _uow.ApplicationRepo.GetAll().Where(a => a.ApplicationId == app.Value).Select(x => new appItemlist { Text = x.Name, Value = x.ApplicationId, Selected = true }).FirstOrDefault();
                                if (!applist.Any(a => a.Value == applicationinfo.Value))
                                {
                                    applist.Add(applicationinfo);
                                }
                            }
                        }
                        else
                        {
                            facility.Selected = false;
                        }
                    }
                    userViewModel.ApplicationList = applist;

                    foreach (var option in userViewModel.ApplicationList)
                    {
                        if (selectedApps.Contains(option.Value.ToString()))
                        {
                            option.Selected = true;
                        }
                    }
                    var OrgList = _uow.OrganizationRepo.GetAll().Select(x => new appItemlist { Text = x.OrganizationName, Value = x.OrganizationID, Selected = false }).ToList();
                    foreach (var org in OrgList)
                    {
                        if (userViewModel.OrgList.Any(o => o.Value == org.Value)) { org.Selected = true; }
                    }
                    userViewModel.OrgList = OrgList;
                }
                var userTypeList = _uow.UserTypeRepo.GetAll().Select(x => new appItemlist { Text = x.Name, Value = x.Id, Selected = false }).ToList();
                foreach (var typeitem in userTypeList)
                {
                    if (typeitem.Value == editUser.UserType)
                    {
                        typeitem.Selected = true;
                    }
                }
                userViewModel.UserTypeList = userTypeList;
                ViewBag.userType = editUser.UserType;
                ViewBag.editingAnotherUser = false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in User Controller/ My Profile Edit Information:{ErrorMsg}", ex.Message);
            }
            return View("~/Areas/TANUserManagement/Views/User/EditUser.cshtml", userViewModel);
        }
        [HttpPost]
        public async Task<ActionResult> UpdateUser(string userName, string Fname, string Lname, string email, string[] facilities, string defaultApp, bool twofa, string userId)
        {
            try
            {
                var loggedInUserInfo = await _userManager.GetUserAsync(HttpContext.User);
                //var user = await _userManager.FindByEmailAsync(email);
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    AspNetUser aspNetUser = new AspNetUser();
                    if (userName != null && user.UserName != userName)
                    {
                        user.UserName = userName;
                    }
                    if (email != null && user.Email != email)
                    {
                        user.Email = email;
                    }
                    if (Fname != null && user.FirstName != Fname)
                    {
                        user.FirstName = Fname;
                    }
                    if (Lname != null && user.LastName != Lname)
                    {
                        user.LastName = Lname;
                    }
                    if (user.TwoFactorEnabled != twofa)
                    {
                        user.TwoFactorEnabled = twofa;
                    }
                    if (defaultApp != null && user.DefaultAppId != Convert.ToInt32(defaultApp))
                    {
                        user.DefaultAppId = Convert.ToInt32(defaultApp);
                    }
                    if (user.UserType == Convert.ToInt32(UserTypes.Other))
                    {

                        var userExistingOrganizationFacilitiesInfo = _uow.UserOrganizationFacilitiesRepo.GetAll().Where(x => x.UserId == user.Id).ToList();

                        if (userExistingOrganizationFacilitiesInfo != null)
                        {
                            foreach (var userExistingFacility in userExistingOrganizationFacilitiesInfo)
                            {
                                userExistingFacility.IsActive = false;
                                userExistingFacility.UpdatedDate = DateTime.UtcNow;
                                userExistingFacility.UpdatedBy = loggedInUserInfo.Id;
                            }

                            foreach (var currentFacility in facilities)
                            {
                                var existingFacility = userExistingOrganizationFacilitiesInfo.FirstOrDefault(ex => ex.FacilityID == Convert.ToInt32(currentFacility));
                                if (existingFacility != null)
                                {
                                    existingFacility.UpdatedBy = loggedInUserInfo.Id;
                                    existingFacility.UpdatedDate = DateTime.UtcNow;
                                    existingFacility.IsActive = true;

                                    _uow.UserOrganizationFacilitiesRepo.Update(existingFacility);
                                    _uow.SaveChanges();
                                }
                                else
                                {
                                    UserOrganizationFacility userOrganizationFacility = new();
                                    userOrganizationFacility.UserId = user.Id;
                                    var organization = _uow.FacilityRepo.GetAll().Where(f => f.Id == Convert.ToInt32(currentFacility)).Select(p => new appItemlist { Text = p.FacilityID, Value = p.OrganizationId }).FirstOrDefault();
                                    userOrganizationFacility.OrganizationID = organization.Value;
                                    userOrganizationFacility.FacilityID = Convert.ToInt32(currentFacility);
                                    userOrganizationFacility.CreatedBy = loggedInUserInfo.Id;
                                    userOrganizationFacility.CreatedDate = DateTime.UtcNow;
                                    userOrganizationFacility.IsActive = true;

                                    _uow.UserOrganizationFacilitiesRepo.Add(userOrganizationFacility);
                                    _uow.SaveChanges();
                                }
                            }
                        }

                        var userNewOrganizationFacilitiesInfo = _uow.UserOrganizationFacilitiesRepo.GetAll().Where(x => x.UserId == user.Id && x.IsActive).ToList();

                        var userExistingApplications = _uow.UserApplicationRepo.GetAll().Where(x => x.UserId == user.Id).ToList();

                        foreach (var removeApp in userExistingApplications)
                        {
                            _uow.UserApplicationRepo.Delete(removeApp);
                            _uow.SaveChanges();
                        }

                        foreach (var activeFacility in userNewOrganizationFacilitiesInfo)
                        {
                            var activeApps = _uow.FacilityApplicationRepo.GetAll().Where(aa => aa.FacilityId == activeFacility.FacilityID && aa.IsActive).ToList();

                            foreach (var activeapp in activeApps)
                            {
                                var existingapp = _uow.UserApplicationRepo.GetAll().Where(app => app.UserId == user.Id && app.ApplicationId == activeapp.ApplicationId).FirstOrDefault();
                                if (existingapp == null)
                                {
                                    UserApplication userApplication = new();
                                    userApplication.UserId = user.Id;
                                    userApplication.ApplicationId = activeapp.ApplicationId;
                                    _uow.UserApplicationRepo.Add(userApplication);
                                    _uow.SaveChanges();
                                }

                            }
                        }

                    }
                    else
                    {
                        var userExistingApplications = _uow.UserApplicationRepo.GetAll().Where(x => x.UserId == user.Id).ToList();

                        foreach (var removeApp in userExistingApplications)
                        {
                            _uow.UserApplicationRepo.Delete(removeApp);
                            _uow.SaveChanges();
                        }

                        var totalApplist = _uow.ApplicationRepo.GetAll();
                        foreach (var app in totalApplist)
                        {
                            UserApplication userApplication = new UserApplication();
                            userApplication.ApplicationId = Convert.ToInt32(app.ApplicationId);
                            userApplication.UserId = user.Id;
                            _uow.UserApplicationRepo.Add(userApplication);
                            _uow.SaveChanges();
                        }

                    }
                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        TempData["SuccessmsgPop"] = TANResource.SuccessMsg.ToString();
                    }
                    else
                    {
                        TempData["ErrormsgPop"] = TANResource.ErrorMsg.ToString();
                    }
                }

                if (loggedInUserInfo.Id == userId)
                {
                    return RedirectToAction("MyProfile");
                }
                else
                {
                    return RedirectToAction("UserList");
                }

            }
            catch (Exception ex)
            {
                TempData["ErrormsgPop"] = TANResource.ErrorMsg.ToString();
                Log.Error(ex, "An Error Occured in User Controller / Update User :{ErrorMsg}", ex.Message);
            }
            return RedirectToAction("UserList");
        }

        public JsonResult GetFacilityList(string? OrgId)
        {
            try
            {
                var facilityList = _uow.FacilityRepo.GetAll().Where(c => c.OrganizationId == Convert.ToInt32(OrgId) && c.IsActive).Select(p => new Itemlist { Text = p.FacilityName, Value = p.Id }).ToList();
                return Json(facilityList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in User Controller / Getting Facility List :{ErrorMsg}", ex.Message);
                return Json(new FacilityModel());
            }
        }
        [HttpGet]
        public JsonResult GetFacilityApplicationList(string[] apps)
        {
            List<string> filteredApp = new List<string>();
            List<Itemlist> appList = new List<Itemlist>();
            try
            {
                foreach (var app in apps)
                {
                    var applist = _uow.FacilityApplicationRepo.GetAll().Where(x => x.FacilityId == Convert.ToInt32(app) && x.IsActive).ToList();
                    if (applist != null)
                    {
                        foreach (var item in applist)
                        {
                            if (!filteredApp.Contains(item.ApplicationId.ToString()))
                            {
                                filteredApp.Add(item.ApplicationId.ToString());
                            }
                        }
                    }

                }
                foreach (var item in filteredApp)
                {
                    var newList = _uow.ApplicationRepo.GetAll().Where(x => x.ApplicationId == Convert.ToInt32(item)).Select(p => new Itemlist
                    {
                        Text = p.Name,
                        Value = p.ApplicationId
                    }).ToList();
                    appList.AddRange(newList);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in User Controller / Getting Facility Applications :{ErrorMsg}", ex.Message);
            }
            return Json(appList);
        }

        public JsonResult GetApplication()
        {
            try
            {
                var appList = _uow.ApplicationRepo.GetAll().Select(x => new Itemlist { Text = x.Name, Value = x.ApplicationId }).ToList();
                return Json(appList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in User Controller / Getting Applications :{ErrorMsg}", ex.Message);
                return Json(new Application());
            }
        }
        public JsonResult GetRoles(string? OrgId)
        {
            try
            {
                var appList = _uow.AspNetRolesRepo.GetAll().Where(x => x.ApplicationId == OrgId && x.Name != Roles.SuperAdmin.ToString()).Select(x => new roleItemlist { Text = x.Name, Value = x.Id.ToString() }).ToList();
                return Json(appList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in User Controller / Getting Roles :{ErrorMsg}", ex.Message);
                return Json(new AspNetRoles());
            }
        }
        public async Task<IActionResult> CheckForUniqueEmail(string emailId, string userId)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(emailId);

                if (user != null)
                {
                    if (user.Id == userId)
                    {
                        return Json(false);
                    }
                    else
                    {
                        return Json(true);
                    }
                }
                else
                {
                    return Json(false);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in User Controller / Checking For Valid Email :{ErrorMsg}", ex.Message);
                return Json(false);
            }
        }

        public IActionResult GetValues(string? AddUserId, string? fromFacility)
        {
            try
            {
                TempData["fromFacility"] = fromFacility;
                TempData["AddUserId"] = AddUserId;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in User Controller / Getting Index Values :{ErrorMsg}", ex.Message);
            }
            return RedirectToAction("UserList");
        }

        [Authorize(Permissions.UserManagement.PBJSnapEdit)]
        public async Task<IActionResult> Resend(string UserId)
        {
            var user = _userManager.Users.Where(x => x.Id == UserId).FirstOrDefault();
            var response = string.Empty;
            try
            {
                Utils utils = new Utils();
                SendMail sendMail = new();
                EmailTemplateViewModel emailTemplateVM = new();
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                string password = utils.CreateDummyPassword();
                List<string> recipients = new List<string> { user.Email };
                string server = _configuration.GetValue<string>("DomainPath:Server");
                string logoPath = _configuration.GetValue<string>("DomainPath:LogoPath");
                if (user != null)
                {
                    if (user.IsUsingTempPassword == 0)
                    {
                        var result = await _userManager.ResetPasswordAsync(user, code, password);
                        if (result.Succeeded)
                        {
                            emailTemplateVM.DummyPassword = password;
                            emailTemplateVM.MailFormat = "Password";
                            emailTemplateVM.TemplateFilePath = TANResource.ResetPasswordEmailTemplatePath;
                            emailTemplateVM.Message = "Password.";
                            emailTemplateVM.UserName = user.FirstName;
                            emailTemplateVM.LogoPath = server + logoPath + "logo1.png";
                            emailTemplateVM.ForgotPasswordLogoPath = server + logoPath + "forgot-the-pass.png";
                            sendMail.Recipients = recipients.ToArray();
                            sendMail.Subject = "Think Anew Email Confirmation";
                            sendMail.Body = await _mailContentHelper.CreateMailContent(emailTemplateVM);
                            bool isMailSent = await _emailHelper.SendEmailAsync(sendMail);
                            if (isMailSent)
                            {
                                user.PasswordHash = password;
                                response = "New password has been sent";
                            }
                            else
                            {
                                response = string.Empty;
                                Log.Error(response, "An Error Occured in User Controller / Reset :{ErrorMsg}", response);
                            }
                        }
                        else
                        {
                            response = string.Empty;
                            Log.Error(response, "An Error Occured in User Controller / Reset :{ErrorMsg}", response);

                        }
                    }
                    else
                    {

                        var callbackUrl = Url.Page(
                           "/Account/ResetPassword",
                           pageHandler: null,
                           values: new { area = "Identity", code },
                           protocol: Request.Scheme);

                        emailTemplateVM.Url = callbackUrl;
                        emailTemplateVM.MailFormat = "URL";
                        emailTemplateVM.TemplateFilePath = TANResource.ForgotPasswordEmailTemplatePath;
                        emailTemplateVM.Message = "Link.";
                        emailTemplateVM.UserName = user.FirstName;
                        emailTemplateVM.LogoPath = server + logoPath + "logo1.png";
                        emailTemplateVM.ForgotPasswordLogoPath = server + logoPath + "forgot-the-pass.png";
                        sendMail.Recipients = recipients.ToArray();
                        sendMail.Subject = "Reset Your Think Anew Reset Password";
                        sendMail.Body = await _mailContentHelper.CreateMailContent(emailTemplateVM);
                        //await _mailHelper.SendMail(sendMail);
                        bool isMailSent = await _emailHelper.SendEmailAsync(sendMail);
                        if(isMailSent)
                        {
                            response = "Update Paswword Link has been Sent";
                        }
                        else
                        {
                            response = string.Empty;
                            Log.Error(response, "An Error Occured in User Controller / Reset :{ErrorMsg}", response);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                response = string.Empty;
                Log.Error(ex, "An Error Occured in User Controller / Reset :{ErrorMsg}", ex.Message);
            }
            return Json(response);
        }

    }
}
