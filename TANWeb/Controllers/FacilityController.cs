using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using TAN.DomainModels;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TAN.Repository.Implementations;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using TAN.DomainModels.Helpers;
using Serilog;
using System.Linq.Expressions;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Identity;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.RateLimiting;
using TANWeb.Helpers;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace TANWeb.Controllers
{
    public class FacilityController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<AspNetUser> _userManager;


        public FacilityController(IUnitOfWork uow, UserManager<AspNetUser> userManager)
        {
            _uow = uow;
            _userManager = userManager;
        }

        [Route("/facilities")]
        public async Task<IActionResult> Index()
        {
            FacilityViewModel facilityViewModel = new();
            var selectedApp = HttpContext.Session.GetString("SelectedApp");
            facilityViewModel.OrganizationList = await _uow.OrganizationRepo.GetApplicationWiseOrganizations(selectedApp);
            facilityViewModel.StateList = _uow.StateRepo.GetAll().Select(s => new StateModel { StateName = s.StateName, StateCode = s.StateCode }).ToList();
            facilityViewModel.ApplicationList = _uow.ApplicationRepo.GetAll().Select(a => new Itemlist { Text = a.Name, Value = a.ApplicationId }).ToList();
            if (TempData.TryGetValue("buttonId", out var buttonId))
            {
                ViewBag.buttonId = buttonId;
            }
            if (TempData.TryGetValue("isFromOrganization", out var isFromOrganization))
            {
                ViewBag.isfromOrganization = isFromOrganization;
            }
            bool isThinkAnewUser = _uow.FacilityRepo.GetLoggedInUserInfo();
            if (!isThinkAnewUser)
            {
                var organizationInfoDetails = _uow.FacilityRepo.GetOrganizationUserDetails();
                facilityViewModel.OrganizationList = _uow.OrganizationRepo.GetAll().Where(o => o.IsActive).Select(a => new Itemlist { Text = a.OrganizationName, Value = a.OrganizationID }).ToList();
                if (organizationInfoDetails != null)
                {
                    facilityViewModel.OrganizationList = facilityViewModel.OrganizationList.Where(o => o.Value.Equals(Convert.ToInt32(organizationInfoDetails))).ToList();
                }
            }
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            facilityViewModel.UserType = currentUser.UserType;
            return View(facilityViewModel);
        }

        public IActionResult GetValues(string? buttonId, string? isFromOrganization)
        {
            TempData["isFromOrganization"] = isFromOrganization;
            TempData["buttonId"] = buttonId;

            return RedirectToAction("Index");
        }

        [Authorize(Permissions.Facility.PBJSnapCreate)]
        [HttpPost]
        [EnableRateLimiting("AddPolicy")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SaveFacility(FacilityViewModel facility)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = _uow.BeginTransaction())
                {
                    try
                    {
                        string result = await _uow.FacilityRepo.AddFacility(facility);
                        if (result != null && result == "success" && !string.IsNullOrEmpty(facility.FacilityID) && (facility.HasDeduction))
                        {
                            var facilityID = _uow.FacilityRepo.GetAll().FirstOrDefault(x => x.FacilityID == facility.FacilityID).Id;
                            var check = _uow.BreakDeductionRepo.GetAll().FirstOrDefault(x => x.FacilityId == facilityID);
                            if (check == null)
                            {
                                BreakDeduction bd = new BreakDeduction();
                                bd.FacilityId = facilityID;
                                bd.RegularTime = Convert.ToInt32(facility.RegularDeduction);
                                bd.OverTime = Convert.ToInt32(facility.OverTimeDeduction);
                                bd.CreatedTime = DateTime.UtcNow;
                                _uow.BreakDeductionRepo.Add(bd);
                                _uow.SaveChanges();
                            }
                            transaction.Commit();
                            return Json(result);
                        }
                        else
                        {
                            transaction.Rollback();
                            return Json(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "An Error Occured in Facility Controller / Adding New Facility Details :{ErrorMsg}", ex.Message);
                        transaction.Rollback();
                        return Json("failed");
                    }
                }
            }
            else
            {
                return Json("failed");
            }
        }

        [Authorize(Permissions.Facility.PBJSnapView)]
        [HttpPost]
        public JsonResult GetFacilities(string orgName, string appName, string storedValue, string searchValue, string start, string length, string sortColumn, string sortDirection, int draw, string orgId)
        {
            int totalRecord = 0;
            int filterRecord = 0;
            if (string.IsNullOrEmpty(orgName) && !string.IsNullOrEmpty(storedValue))
            {
                orgName = storedValue.Trim();

            }

            int pageSize = Convert.ToInt32(length ?? "0");
            int skip = Convert.ToInt32(start ?? "0");
            string sortOrder = string.Empty;

            IEnumerable<FacilityViewModel> data;

            int facilityCount = 0;


            //sort data
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
            {
                sortOrder = (sortColumn + "_" + sortDirection).ToLower();
            }

            data = _uow.FacilityRepo.GetAllFacilities(skip, pageSize, searchValue, sortOrder, orgName, appName, orgId, out facilityCount);

            totalRecord = facilityCount;

            // get total count of records after search 
            filterRecord = totalRecord;

            var returnObj = new { draw = draw, recordsTotal = totalRecord, recordsFiltered = filterRecord, data = data.ToList() };
            return Json(returnObj);
        }

        [Authorize(Permissions.Facility.PBJSnapEdit)]
        public async Task<IActionResult> EditFacility(int facilityId)
        {
            try
            {
                var facilityViewModel = await _uow.FacilityRepo.GetFacilityDetail(facilityId);
                return Json(facilityViewModel);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Facility Controller / Edit Existing Facility Details :{ErrorMsg}", ex.Message);
                return Json(new FacilityViewModel());
            }
        }

        [Authorize(Permissions.Facility.PBJSnapEdit)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateFacility(FacilityViewModel facilityViewModel)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = _uow.BeginTransaction())
                {
                    try
                    {
                        var result = await _uow.FacilityRepo.UpdateFacility(facilityViewModel);
                        if (result == "success")
                        {
                            if (facilityViewModel.HasDeduction)
                            {
                                var exist = _uow.BreakDeductionRepo.GetAll().Where(x => x.FacilityId == facilityViewModel.Id).FirstOrDefault();
                                if (exist != null)
                                {
                                    exist.RegularTime = (int)(facilityViewModel.RegularDeduction != null ? facilityViewModel.RegularDeduction : 0);
                                    exist.OverTime = (int)(facilityViewModel.OverTimeDeduction != null ? facilityViewModel.OverTimeDeduction : 0);
                                    exist.UpdatedTime = DateTime.UtcNow;
                                    _uow.BreakDeductionRepo.Update(exist);
                                }
                                else
                                {
                                    BreakDeduction breakDeduction = new BreakDeduction();
                                    breakDeduction.FacilityId = facilityViewModel.Id;
                                    breakDeduction.RegularTime = (int)(facilityViewModel.RegularDeduction != null ? facilityViewModel.RegularDeduction : 0);
                                    breakDeduction.OverTime = (int)(facilityViewModel.OverTimeDeduction != null ? facilityViewModel.OverTimeDeduction : 0);
                                    breakDeduction.CreatedBy = facilityViewModel.CreatedBy;
                                    breakDeduction.CreatedTime = DateTime.UtcNow;
                                }
                            }
                            transaction.Commit();
                            return Json(result);
                        }
                        else
                        {
                            transaction.Rollback();
                            return Json(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "An Error Occured in Facility Controller / Updating Existing Facility Details :{ErrorMsg}", ex.Message);
                        transaction.Rollback();
                        return Json("failed");
                    }
                }
            }
            else
            {
                return Json("failed");
            }
        }

        [Authorize(Permissions.Facility.PBJSnapDelete)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFacility(int facilityId)
        {
            using (var transaction = _uow.BeginTransaction())
            {
                try
                {
                    var result = await _uow.FacilityRepo.DeleteFacilityDetail(facilityId);
                    if (result == "success")
                    {
                        transaction.Commit();
                        return Json(result);
                    }
                    else
                    {
                        transaction.Rollback();
                        return Json(result);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An Error Occured in Facility Controller / Deleting Facility Details :{ErrorMsg}", ex.Message);
                    transaction.Rollback();
                    return Json("failed");
                }
            }
        }

        [HttpGet]
        public IActionResult CheckUserRole()
        {
            try
            {
                bool hasRole = _uow.FacilityRepo.CheckRole();
                return Json(hasRole);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Facility Controller / Checking For User Is In SuperAdmin Role :{ErrorMsg}", ex.Message);
                return Json(false);
            }
        }

        public async Task<IActionResult> CheckForValidName(string facilityName, string facilityId)
        {
            try
            {
                bool isNameExists = await _uow.FacilityRepo.GetFacilityName(facilityName, facilityId);
                return Json(isNameExists);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Facility Controller / Checking For Valid Facility Name :{ErrorMsg}", ex.Message);
                return Json(false);
            }
        }


        public async Task<IActionResult> CheckForUniqueId(string facilityId)
        {
            try
            {
                bool isIdExists = await _uow.FacilityRepo.GetFacilityId(facilityId);
                return Json(isIdExists);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Facility Controller / Checking For Unique Facility Id :{ErrorMsg}", ex.Message);
                return Json(false);
            }
        }

        public IActionResult GetOrganizationServiceDetails(int organizationId)
        {
            try
            {
                var orgServiceDetails = _uow.FacilityRepo.GetOrganizationServiceDetails(organizationId);
                if (orgServiceDetails.OrganizationId >= 0)
                {
                    return Json(orgServiceDetails);
                }
                return Json(null);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Facility Controller / Getting Organization Service Details :{ErrorMsg}", ex.Message);
                return Json(null);
            }
        }

        public IActionResult GetOrganizationUserInfo()
        {
            try
            {
                var organizationId = _uow.FacilityRepo.GetOrganizationUserDetails();
                return Json(organizationId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Facility Controller / Getting Organization User Details :{ErrorMsg}", ex.Message);
                return Json(null);
            }
        }
    }
}
