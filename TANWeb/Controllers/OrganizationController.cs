using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Extensibility;
using TAN.DomainModels.Entities;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using System.Linq.Expressions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Serilog;
using TAN.DomainModels.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using TANWeb.Helpers;

namespace TANWeb.Controllers
{
    //  [Authorize(Roles="SuperAdmin")]
    public class OrganizationController : Controller
    {
        private readonly IUnitOfWork _uow;
        public OrganizationController(IUnitOfWork uow)
        {
            _uow = uow;
        }
        [Route("/organizations")]
        public IActionResult Index()
        {
            OrganizationViewModel organizationViewModel = new OrganizationViewModel();
            organizationViewModel.StateList = _uow.StateRepo.GetAll().Select(s => new StateModel { StateName = s.StateName, StateCode = s.StateCode }).ToList();
            return View(organizationViewModel);
        }

        [Authorize(Permissions.Organization.PBJSnapCreate)]
        [HttpPost]
        [EnableRateLimiting("AddPolicy")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveOrganization(OrganizationViewModel organization)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = _uow.BeginTransaction())
                {
                    try
                    {
                        var result = await _uow.OrganizationRepo.AddOrganization(organization);
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
                        Log.Error(ex, "An Error Occured in Organization Controller / Adding New Organization Details :{ErrorMsg}", ex.Message);
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

        [Authorize(Permissions.Organization.PBJSnapEdit)]
        public async Task<IActionResult> EditOrganization(int organizationId)
        {
            try
            {
                var organizationViewModel = await _uow.OrganizationRepo.GetOrganizationDetail(organizationId);
                return Json(organizationViewModel);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Organization Controller / Editing Organization Details :{ErrorMsg}", ex.Message);
                return Json(new OrganizationViewModel());
            }
        }

        [Authorize(Permissions.Organization.PBJSnapEdit)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrganization(OrganizationViewModel organizationViewModel)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = _uow.BeginTransaction())
                {
                    try
                    {
                        var result = await _uow.OrganizationRepo.UpdateOrganization(organizationViewModel);
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
                        Log.Error(ex, "An Error Occured in Organization Controller / Updating Organization Details :{ErrorMsg}", ex.Message);
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

        [Authorize(Permissions.Organization.PBJSnapDelete)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOrganization(int organizationId)
        {
            using (var transaction = _uow.BeginTransaction())
            {
                try
                {
                    var result = await _uow.OrganizationRepo.DeleteOrganizationDetail(organizationId);
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
                    Log.Error(ex, "An Error Occured in Organization Controller / Deleting Organization Details :{ErrorMsg}", ex.Message);
                    transaction.Rollback();
                    return Json("failed");
                }
            }
        }

        [Authorize(Permissions.Organization.PBJSnapView)]
        [HttpPost]
        public JsonResult GetOrganizationsList(string searchValue, string start, string length, string sortColumn, string sortDirection, int draw, string serviceType)
        {
            int totalRecord = 0;
            int filterRecord = 0;

            int pageSize = Convert.ToInt32(length ?? "0");
            int skip = Convert.ToInt32(start ?? "0");
            string sortOrder = string.Empty;

            IEnumerable<OrganizationViewModel> data;

            int organizationCount = 0;


            //sort data
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
            {
                sortOrder = (sortColumn + "_" + sortDirection).ToLower();
            }

            data = _uow.OrganizationRepo.GetAllOrganizations(skip, pageSize, searchValue, sortOrder, serviceType, out organizationCount);

            totalRecord = organizationCount;

            // get total count of records after search 
            filterRecord = organizationCount;

            var returnObj = new { draw = draw, recordsTotal = totalRecord, recordsFiltered = filterRecord, data = data.ToList() };
            return Json(returnObj);
        }

        public async Task<IActionResult> CheckForValidEmail(string emailId, int organizationId)
        {
            try
            {
                bool isEmailExists = await _uow.OrganizationRepo.GetEmailId(emailId, organizationId);
                return Json(isEmailExists);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Organization Controller / Checking For Valid Email Id :{ErrorMsg}", ex.Message);
                return Json(false);
            }
        }

        public async Task<IActionResult> CheckForValidName(string organizationName, int organizationId)
        {
            try
            {
                bool isNameExists = await _uow.OrganizationRepo.GetOrganizationName(organizationName, organizationId);
                return Json(isNameExists);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Organization Controller / Checking For Valid Name :{ErrorMsg}", ex.Message);

                return Json(false);
            }
        }

        public IActionResult GetMasterAccountDetails()
        {
            try
            {
                var bandWidthMasterDetails = _uow.OrganizationRepo.GetMasterAccountDetails();
                if (bandWidthMasterDetails != null)
                {
                    return Json(bandWidthMasterDetails);
                }
                return Json(bandWidthMasterDetails);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Organization Controller / Get Master Account Details :{ErrorMsg}", ex.Message);
                return Json(null);
            }
        }

        public async Task<IActionResult> CheckForSubAccountId(int organizationId, string subAccountId)
        {
            try
            {
                bool isSubAccountIdExists = await _uow.OrganizationRepo.CheckSubAccountId(organizationId, subAccountId);
                return Json(isSubAccountIdExists);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Organization Controller / Checking For Unique Sub-Account Id :{ErrorMsg}", ex.Message);

                return Json(false);
            }
        }
    }
}
