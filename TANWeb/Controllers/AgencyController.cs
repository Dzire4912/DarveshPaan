using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.DotNet.MSIdentity.Shared;
using Microsoft.EntityFrameworkCore.Storage;
using Serilog;
using System.Linq.Expressions;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TAN.Repository.Implementations;
using TANWeb.Helpers;
using static TAN.DomainModels.Helpers.Permissions;

namespace TANWeb.Controllers
{
    public class AgencyController : Controller
    {
        private readonly IUnitOfWork _uow;
        public AgencyController(IUnitOfWork uow)
        {
            _uow = uow;
        }
        [Route("/agencies")]
        public async Task<IActionResult> Index()
        {
            AgencyViewModel agencyViewModel = new AgencyViewModel();

            agencyViewModel.StateList = _uow.StateRepo.GetAll().Select(s => new StateModel { StateName = s.StateName, StateCode = s.StateCode.ToString() }).ToList();

            //agencyViewModel.FacilityList = _uow.FacilityRepo.GetAll().Where(f => f.IsActive).Select(f => new Itemlist { Text = f.FacilityName, Value = f.Id }).ToList();
            agencyViewModel.FacilityList = await _uow.FacilityRepo.GetAllFacilitiesByOrgId();
            agencyViewModel.AgencyIdList = _uow.AgencyRepo.GetAll().Where(a => a.IsActive).Select(a => new Itemlist { Text = a.AgencyId, Value = a.Id }).DistinctBy(a => a.Text).ToList();

            bool isThinkAnewUser = _uow.FacilityRepo.GetLoggedInUserInfo();
            if (!isThinkAnewUser)
            {
                var organizationInfoDetails = _uow.FacilityRepo.GetOrganizationUserDetails();
                if (organizationInfoDetails != null)
                {
                    var userFacilityInfo = _uow.FacilityRepo.GetAll().Where(f => f.OrganizationId == Convert.ToInt32(organizationInfoDetails)).ToList();
                    agencyViewModel.FacilityList = userFacilityInfo;
                    agencyViewModel.AgencyIdList.Clear();
                    foreach (var userFacility in userFacilityInfo)
                    {
                        var userAgencies = _uow.AgencyRepo.GetAll().Where(a => a.FacilityId == userFacility.Id && a.IsActive).Select(a => new Itemlist { Text = a.AgencyId, Value = a.Id }).DistinctBy(a => a.Text).ToList();
                        agencyViewModel.AgencyIdList.AddRange(userAgencies);
                    }
                }
            }
            return View(agencyViewModel);
        }

        [Authorize(Permissions.Agency.PBJSnapCreate)]
        [HttpPost]
        [EnableRateLimiting("AddPolicy")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAgency(AgencyViewModel agency)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = _uow.BeginTransaction())
                {
                    try
                    {
                        var result = await _uow.AgencyRepo.AddAgency(agency);
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
                        Log.Error(ex, "An Error Occured in Agency Controller / Adding New Agency Details :{ErrorMsg}", ex.Message);
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

        [Authorize(Permissions.Agency.PBJSnapView)]
        [HttpPost]
        public JsonResult GetAllAgencies(string facilityName, string agencyId, string searchValue, string start, string length, string sortColumn, string sortDirection, int draw, string organizationId)
        {
            int totalRecord = 0;
            int filterRecord = 0;
            int pageSize = Convert.ToInt32(length ?? "0");
            int skip = Convert.ToInt32(start ?? "0");
            string sortOrder = string.Empty;

            IEnumerable<AgencyViewModel> data;

            int agencyCount = 0;

            //sort data
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
            {
                sortOrder = (sortColumn + "_" + sortDirection).ToLower();
            }

            data = _uow.AgencyRepo.GetAllAgenciesList(skip, pageSize, searchValue, sortOrder, facilityName, agencyId, organizationId, out agencyCount);

            totalRecord = agencyCount;

            // get total count of records after search 
            filterRecord = totalRecord;

            var returnObj = new { draw = draw, recordsTotal = totalRecord, recordsFiltered = filterRecord, data = data.ToList() };
            return Json(returnObj);
        }


        [Authorize(Permissions.Agency.PBJSnapEdit)]
        public async Task<IActionResult> GetAgencyDetail(string agencyId)
        {
            try
            {
                var agencyDetail = await _uow.AgencyRepo.GetAgencyDetails(agencyId);
                return Json(agencyDetail);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Agency Controller / Editing Agency Details :{ErrorMsg}", ex.Message);
                return Json(new AgencyViewModel());
            }
        }

        [Authorize(Permissions.Agency.PBJSnapEdit)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAgency(AgencyViewModel agencyViewModel)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = _uow.BeginTransaction())
                {
                    try
                    {
                        var result = await _uow.AgencyRepo.UpdateAgency(agencyViewModel);
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
                        Log.Error(ex, "An Error Occured in Agency Controller / Updating Existing Agency Details :{ErrorMsg}", ex.Message);
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

        [Authorize(Permissions.Agency.PBJSnapDelete)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAgency(int agencyId)
        {
            try
            {
                var result = await _uow.AgencyRepo.DeleteAgencyDetail(agencyId);
                return Json(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Agency Controller / Delete Existing Agency :{ErrorMsg}", ex.Message);
                return Json("failed");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetAgenciesByFacility(int FacilityId)
        {
            List<AgencyModel> agencies = new List<AgencyModel>();
            try
            {
                agencies = await _uow.AgencyRepo.GetAllAgeniciesByFacilityId(Convert.ToInt32(FacilityId));
            }
            catch (Exception ex)
            {
                Log.Error("GetAgenciesByFacility ", ex.Message);
            }
            return Json(agencies);
        }

        public async Task<IActionResult> CheckForValidName(string agencyName, string agencyId)
        {
            try
            {
                bool isNameExists = await _uow.AgencyRepo.GetAgencyName(agencyName, agencyId);
                return Json(isNameExists);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Agency Controller / Checking For Valid Agency Name :{ErrorMsg}", ex.Message);
                return Json(false);
            }
        }

        public async Task<IActionResult> CheckForUniqueId(string agencyId)
        {
            try
            {
                bool isIdExists = await _uow.AgencyRepo.GetAgencyId(agencyId);
                return Json(isIdExists);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Agency Controller / Checking For Unique Agency Id :{ErrorMsg}", ex.Message);
                return Json(false);
            }
        }

        public JsonResult GetAgencyIdListByFacilityName(string? facilityName)
        {
            try
            {
                var facilityDetails = _uow.FacilityRepo.GetAll().Where(f => f.FacilityName == facilityName).FirstOrDefault();
                if (facilityDetails != null)
                {
                    var agencyIdList = _uow.AgencyRepo.GetAll().Where(a => a.FacilityId == facilityDetails.Id && a.IsActive).Select(a => new Itemlist { Text = a.AgencyId, Value = a.Id }).DistinctBy(a => a.Text).ToList();
                    return Json(agencyIdList);
                }
                else
                {
                    bool isThinkAnewUser = _uow.FacilityRepo.GetLoggedInUserInfo();
                    if (!isThinkAnewUser)
                    {
                        var organizationInfoDetails = _uow.FacilityRepo.GetOrganizationUserDetails();

                        if (organizationInfoDetails != null)
                        {
                            var userFacilityInfo = _uow.FacilityRepo.GetAll().Where(f => f.OrganizationId == Convert.ToInt32(organizationInfoDetails)).ToList();
                            AgencyViewModel agencyViewModel = new AgencyViewModel();
                            agencyViewModel.AgencyIdList = new List<Itemlist>();
                            foreach (var userFacility in userFacilityInfo)
                            {
                                var userAgencies = _uow.AgencyRepo.GetAll().Where(a => a.FacilityId == userFacility.Id && a.IsActive).Select(a => new Itemlist { Text = a.AgencyId, Value = a.Id }).DistinctBy(a => a.Text).ToList();
                                agencyViewModel.AgencyIdList.AddRange(userAgencies);
                            }
                            return Json(agencyViewModel.AgencyIdList);
                        }
                    }
                    var agencyIdList = _uow.AgencyRepo.GetAll().Where(a => a.IsActive).Select(a => new Itemlist { Text = a.AgencyId, Value = a.Id }).DistinctBy(a => a.Text).ToList();
                    return Json(agencyIdList);
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Agency Controller / Getting Agency Ids List :{ErrorMsg}", ex.Message);
                return Json(new AgencyModel());
            }
        }


        public IActionResult GetLoggedInUserInfo()
        {
            try
            {
                bool isThinkAnewUser = _uow.FacilityRepo.GetLoggedInUserInfo();
                return Json(isThinkAnewUser);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Agency Controller / Getting LoggedIn User Information :{ErrorMsg}", ex.Message);

                return Json(false);
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
                Log.Error(ex, "An Error Occured in Agency Controller / Getting Organization Details :{ErrorMsg}", ex.Message);
                return Json(null);
            }
        }
    }
}
