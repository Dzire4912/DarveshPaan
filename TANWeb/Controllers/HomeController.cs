//HomeController
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using Serilog;
using SimpleFeedReader;
using System.Diagnostics;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TANWeb.Models;
using TANWeb.Resources;

namespace TANWeb.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {

        private readonly UserManager<AspNetUser> _userManager;
        private readonly RoleManager<AspNetRoles> _roleManager;
        private readonly SignInManager<AspNetUser> _signInManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        public HomeController(UserManager<AspNetUser> userManager, SignInManager<AspNetUser> signInManager, IUnitOfWork unitOfWork, RoleManager<AspNetRoles> roleManager, IConfiguration configuration)
        {

            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
            _configuration = configuration;
        }
        [Route("pbjsnap/dashboard")]
        public async Task<IActionResult> Index()
        {
            HttpContext.Session.SetString("SelectedApp", "PBJSnap");
            HttpContext.Session.SetString("LastActivity", DateTimeOffset.Now.ToString());
            var dashboardViewModel = new DashboardViewModel();
            var dashboardfacilityViewModelList = new List<DashboardFacilityViewModel>();
            var user = await _userManager.GetUserAsync(User);
            ChartData EmpPayType = await EmployeePayTypePiechart();
            ViewBag.ChartData = EmpPayType;
            ChartData EmpDistribution = await EmployeeDistributionBarchart();
            ViewBag.BarChartData = EmpDistribution;
            ChartData EmpLabourType = await EmployeeLabourTypechart();
            ViewBag.BarChartLabourData = EmpLabourType;
            if (user != null)
            {
                Log.Information("Home Page");
            }
            return View();
        }
        public async Task<ChartData> EmployeeDistributionBarchart()
        {
            var facilityList = await GetFacilitylistAsync();

            ChartData ct = new ChartData();
            var emplist = new List<Itemlist>();
            try
            {
                if (facilityList != null)
                {
                    try
                    {
                        foreach (var item in facilityList)
                        {
                            var empCount = _unitOfWork.EmployeeRepo.GetAll().Where(c => c.IsActive).Count(x => x.FacilityId == item.Id);
                            var facilityEmp = new Itemlist { Text = item.FacilityName, Value = empCount };
                            emplist.Add(facilityEmp);
                        }
                        emplist = emplist.OrderByDescending(e => e.Value).Take(15).ToList();
                    }
                    catch (Exception ex)
                    {
                        //Log
                    }
                }
                ct.Labels = emplist.Select(item => item.Text).ToArray();
                ct.Data = emplist.Select(item => item.Value).ToArray();

            }
            catch (Exception ex)
            {
                //log
            }
            return ct;
        }
        public async Task<ChartData> EmployeeLabourTypechart()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            int orgId = 0;
            if (user != null && user.UserType != 1)
            {
                var userOrg = _unitOfWork.UserOrganizationFacilitiesRepo.GetAll().Where(x => x.UserId == user.Id && x.IsActive).FirstOrDefault();
                orgId = userOrg.OrganizationID;
            }
            ChartData ct = new ChartData();
            List<Itemlist> emplist = new List<Itemlist>();
            try
            {
                emplist = await _unitOfWork.EmployeeRepo.GetEmployeeLabourData(orgId);
                ct.Labels = emplist.Select(item => item.Text).ToArray();
                ct.Data = emplist.Select(item => item.Value).ToArray();
            }
            catch (Exception ex)
            {
                //log
            }
            return ct;
        }
        public async Task<IActionResult> GetLabourcodeTable(int facilityId = 0, int fiscal = 0, int year = 0, int month = 0)
        {
            var item = new List<Itemlist>();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            int orgId = 0;
            if (year != 0)
            {
                switch (year)
                {
                    case 1:
                        var today = DateTime.Today;
                        year = today.Year;
                        break;
                    case 2:
                        var lastYear = DateTime.Now.AddYears(-1);
                        year = lastYear.Year;
                        break;
                    default:
                        var lastTwo = DateTime.Now.AddYears(-2);
                        year = lastTwo.Year;
                        break;
                }
            }
            if (user != null && user.UserType != 1)
            {
                var userOrg = _unitOfWork.UserOrganizationFacilitiesRepo.GetAll().Where(x => x.UserId == user.Id && x.IsActive).FirstOrDefault();
                orgId = userOrg.OrganizationID;
            }
            item = await _unitOfWork.EmployeeRepo.GetEmployeeLabourTable(orgId, facilityId, fiscal, year, month);

            List<Itemlist> facilitylist = await GetFacilityItemList();
            ViewBag.FacilityList = facilitylist;
            return PartialView("_LabourCodeTable", item);
        }
        public async Task<IActionResult> GetTopJobCode(int facilityId = 0, int fiscal = 0, int year = 0, int month = 0)
        {
            var item = new List<Itemlist>();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            int orgId = 0;
            if (year != 0)
            {
                switch (year)
                {
                    case 1:
                        var today = DateTime.Today;
                        year = today.Year;
                        break;
                    case 2:
                        var lastYear = DateTime.Now.AddYears(-1);
                        year = lastYear.Year;
                        break;
                    default:
                        var lastTwo = DateTime.Now.AddYears(-2);
                        year = lastTwo.Year;
                        break;
                }
            }
            if (user != null && user.UserType != 1)
            {
                var userOrg = _unitOfWork.UserOrganizationFacilitiesRepo.GetAll().Where(x => x.UserId == user.Id && x.IsActive).FirstOrDefault();
                orgId = userOrg.OrganizationID;
            }
            item = await _unitOfWork.EmployeeRepo.GetTopJobTitleTable(orgId, facilityId, fiscal, year, month);
            List<Itemlist> facilitylist = await GetFacilityItemList();
            ViewBag.FacilityList = facilitylist;
            return PartialView("_GetTopJobs", item);
        }
        public async Task<ChartData> EmployeePayTypePiechart()
        {
            var facilityList = await GetFacilitylistAsync();
            int exemptCount = 0;
            int nonExemptCount = 0;
            int contrctCount = 0;
            var exemptDesc = string.Empty;
            var nonExemptDesc = string.Empty;
            var contractdesc = string.Empty;
            ChartData ct = new ChartData();
            try
            {
                if (facilityList != null)
                {
                    try
                    {
                        foreach (var item in facilityList)
                        {
                            var x = _unitOfWork.EmployeeRepo.GetAll().Where(l => l.FacilityId == item.Id && l.IsActive).Count(x => x.PayTypeCode == 1);
                            exemptCount = exemptCount + x;
                            var y = _unitOfWork.EmployeeRepo.GetAll().Where(l => l.FacilityId == item.Id && l.IsActive).Count(x => x.PayTypeCode == 2);
                            nonExemptCount = nonExemptCount + y;
                            var z = _unitOfWork.EmployeeRepo.GetAll().Where(l => l.FacilityId == item.Id && l.IsActive).Count(x => x.PayTypeCode == 3);
                            contrctCount = contrctCount + z;

                        }
                    }
                    catch (Exception ex)
                    {
                        //Log
                    }
                }

                exemptDesc = _unitOfWork.PayTypeCodesRepo.GetAll().Where(l => l.PayTypeCode == 1 && l.IsActive).Select(x => x.PayTypeDescription).FirstOrDefault();
                nonExemptDesc = _unitOfWork.PayTypeCodesRepo.GetAll().Where(l => l.PayTypeCode == 2 && l.IsActive).Select(x => x.PayTypeDescription).FirstOrDefault();
                contractdesc = _unitOfWork.PayTypeCodesRepo.GetAll().Where(l => l.PayTypeCode == 3 && l.IsActive).Select(x => x.PayTypeDescription).FirstOrDefault();

                ct.Labels = new string[] { exemptDesc, nonExemptDesc, contractdesc };
                ct.Data = new int[] { exemptCount, nonExemptCount, contrctCount };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
            }
            return ct;
        }
        public async Task<IActionResult> GetDashboardFacilities()
        {
            var facilityVM = new List<DashboardFacilityViewModel>();
            var facilityList = await GetFacilitylistAsync();
            if (facilityList != null)
            {
                foreach (var item in facilityList)
                {
                    var dashboardfacilityViewModel = new DashboardFacilityViewModel();
                    var emp = await GetEmployee(item.Id.ToString());
                    dashboardfacilityViewModel.Id = item.Id;
                    dashboardfacilityViewModel.FacilityID = item.FacilityID;
                    dashboardfacilityViewModel.FacilityName = item.FacilityName;
                    dashboardfacilityViewModel.Exempt = emp != null ? emp.Where(x => x.PayTypeCode == 1 && x.IsActive).Count() : 0;
                    dashboardfacilityViewModel.NonExempt = emp.Where(x => x.PayTypeCode == 2 && x.IsActive).Count();
                    dashboardfacilityViewModel.Contract = emp.Where(x => x.PayTypeCode == 3 && x.IsActive).Count();
                    dashboardfacilityViewModel.Employees = emp.Count();
                    facilityVM.Add(dashboardfacilityViewModel);
                }
            }
            return PartialView("_GetDashboardFacilities", facilityVM);
        }
        public async Task<List<FacilityModel>> GetFacilitylistAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var FacilityList = new List<FacilityModel>();
            if (user.UserType != null && user.UserType != Convert.ToInt32(UserTypes.Thinkanew))
            {
                var userOrg = _unitOfWork.UserOrganizationFacilitiesRepo.GetAll().Where(x => x.UserId == user.Id && x.IsActive).FirstOrDefault();
                var facilities = _unitOfWork.FacilityRepo.GetAll().Where(x => x.OrganizationId == userOrg.OrganizationID && x.IsActive).ToList();
                FacilityList = facilities;
            }
            else
            {
                var facilities = _unitOfWork.FacilityRepo.GetAll().Where(x => x.IsActive).ToList();
                FacilityList = facilities;
            }
            return FacilityList;
        }
        public async Task<List<Employee>> GetEmployee(string FacilityId)
        {
            var employees = _unitOfWork.EmployeeRepo.GetAll().Where(x => x.FacilityId.ToString() == FacilityId && x.IsActive).ToList();

            return employees;
        }
        public IActionResult KeepSessionAlive()
        {
            // Update some session data to keep the session alive
            return Json(new { success = true });
        }

        public async Task<IActionResult> GetCMSFeed()
        {
            List<Blog> blogs = new List<Blog>();
            try
            {

                List<string> feeds = new List<string>();
                feeds.Add("https://blog.cms.gov/feed/");
                feeds.Add("https://example.blogspot.com/feeds/posts/default");
                FeedReader rss = new FeedReader();
                var items = rss.RetrieveFeeds(feeds);
                foreach (var item in items)
                {
                    var blog = new Blog();
                    blog.Title = item.Title;
                    blog.Description = item.Content;
                    blog.date = item.Date.ToString();
                    blogs.Add(blog);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
                return PartialView("_GetCMSFeed", blogs);
            }
            return PartialView("_GetCMSFeed", blogs);
        }
        public async Task<IActionResult> GetThinkanewTiles()
        {
            var TilesList = new List<TileViewModel>();
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var userOrg = (dynamic)null;
                if (user != null && user.UserType != Convert.ToInt32(UserTypes.Thinkanew))
                {
                    userOrg = _unitOfWork.UserOrganizationFacilitiesRepo.GetAll().Where(x => x.UserId == user.Id && x.IsActive).FirstOrDefault();
                }

                #region Organization
                //Organization
                if (user != null && user.UserType == Convert.ToInt32(UserTypes.Thinkanew))
                {
                    var OrganizationTile = new TileViewModel();
                    OrganizationTile.TileText = "Organizations";
                    OrganizationTile.TileValue = _unitOfWork.OrganizationRepo.GetAll().Where(x => x.IsActive).Count();
                    OrganizationTile.TileID = "organization";
                    OrganizationTile.TileToolTipText = "Total Organization Count";
                    OrganizationTile.TileArrowText = "ri-building-2-line";
                    OrganizationTile.TileSubText = "bg-success";
                    TilesList.Add(OrganizationTile);
                }
                #endregion

                #region Facility
                //Facility
                var FacilityTile = new TileViewModel();
                FacilityTile.TileText = "Facilities";
                if (user != null && user.UserType == Convert.ToInt32(UserTypes.Thinkanew))
                {
                    FacilityTile.TileValue = _unitOfWork.FacilityRepo.GetAll().Where(x => x.IsActive).Count();
                }
                else
                {
                    FacilityTile.TileValue = _unitOfWork.FacilityRepo.GetAll().Where(x => x.OrganizationId == userOrg.OrganizationID && x.IsActive).Count();
                }
                FacilityTile.TileID = "facility";
                FacilityTile.TileToolTipText = "Total Facilities Count";
                FacilityTile.TileArrowText = "ri-settings-line";
                FacilityTile.TileSubText = "bg-warning";
                TilesList.Add(FacilityTile);
                #endregion

                #region Agency
                //Agencies
                var AgencyTile = new TileViewModel();
                AgencyTile.TileText = "Agencies";
                if (user.UserType == Convert.ToDouble(UserTypes.Thinkanew))
                {
                    AgencyTile.TileValue = _unitOfWork.AgencyRepo.GetAll().Where(x => x.IsActive).Count();
                }
                else
                {
                    AgencyTile.TileValue = await _unitOfWork.AgencyRepo.GetFacilityAgencyCount(userOrg.OrganizationID);
                }
                AgencyTile.TileID = "agencies";
                AgencyTile.TileToolTipText = "Total Client Users Count";
                AgencyTile.TileArrowText = "ri-briefcase-4-line";
                AgencyTile.TileSubText = "bg-danger";
                TilesList.Add(AgencyTile);
                #endregion

            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
                return PartialView("_GetThinkanewTiles", TilesList);
            }

            return PartialView("_GetThinkanewTiles", TilesList);
        }
        public async Task<IActionResult> GetSecondaryTiles()
        {
            var TilesList = new List<TileViewModel>();
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var userOrg = (dynamic)null;
                if (user != null && user.UserType != Convert.ToInt32(UserTypes.Thinkanew))
                {
                    userOrg = _unitOfWork.UserOrganizationFacilitiesRepo.GetAll().Where(x => x.UserId == user.Id && x.IsActive).FirstOrDefault();
                }


                #region Employee, ThinkA new users, Client users
                //Employee
                var EmployeeTile = new TileViewModel();
                EmployeeTile.TileText = "Employees";
                if (user != null && user.UserType == Convert.ToInt32(UserTypes.Thinkanew))
                {
                    EmployeeTile.TileValue = _unitOfWork.EmployeeRepo.GetAll().Where(x => x.IsActive).Count();
                }
                else
                {
                    EmployeeTile.TileValue = await _unitOfWork.EmployeeRepo.GetOrganizationEmpCount(userOrg.OrganizationID);
                }

                EmployeeTile.TileID = "employees";
                EmployeeTile.TileToolTipText = "Total Employees Count";
                EmployeeTile.TileArrowText = "ri-user-fill";
                EmployeeTile.TileSubText = "text-danger";
                TilesList.Add(EmployeeTile);
                if (user.UserType == Convert.ToInt32(UserTypes.Thinkanew))
                {
                    //ThinkAnewUser
                    var ThinkAnewUserTile = new TileViewModel();
                    ThinkAnewUserTile.TileText = "ThinkAnew Users";
                    ThinkAnewUserTile.TileValue = _userManager.Users.Where(x => x.UserType == 1 && x.IsActive).Count();
                    ThinkAnewUserTile.TileID = "thinkAnewUser";
                    ThinkAnewUserTile.TileToolTipText = "Total ThinkAnew Users Count";
                    ThinkAnewUserTile.TileArrowText = "ri-group-fill";
                    ThinkAnewUserTile.TileSubText = "text-info";
                    TilesList.Add(ThinkAnewUserTile);
                    //ClientUsers
                    var ClientUsersTile = new TileViewModel();
                    ClientUsersTile.TileText = "Client Users";
                    ClientUsersTile.TileValue = Convert.ToInt64(await _unitOfWork.EmployeeRepo.activeOrgClients());
                    ClientUsersTile.TileID = "clientUsers";
                    ClientUsersTile.TileToolTipText = "Total Client Users Count";
                    ClientUsersTile.TileArrowText = "ri-team-fill";
                    ClientUsersTile.TileSubText = "text-success";
                    TilesList.Add(ClientUsersTile);
                }
                #endregion

                #region Contractors
                //Contractors

                var role = _roleManager.Roles.Where(x => x.NormalizedName == "CONTRACTOR" && x.ApplicationId == 1.ToString() && x.IsActive).FirstOrDefault();
                if (role != null)
                {
                    var ContractorTile = new TileViewModel();
                    ContractorTile.TileText = "Contractors";
                    if (user.UserType == Convert.ToDouble(UserTypes.Thinkanew))
                    {
                        int myOrg = 0;
                        ContractorTile.TileValue = await _unitOfWork.AspNetUserRoleRepo.GetContractorCountByFacility(myOrg);
                    }
                    else
                    {
                        ContractorTile.TileValue = await _unitOfWork.AspNetUserRoleRepo.GetContractorCountByFacility(userOrg.OrganizationID);
                    }
                    ContractorTile.TileID = "contractors";
                    ContractorTile.TileToolTipText = "Total Client Users Count";
                    ContractorTile.TileArrowText = "ri-user-2-fill";
                    ContractorTile.TileSubText = "text-warning";
                    TilesList.Add(ContractorTile);
                }
                else
                {
                    var ContractorTile = new TileViewModel();
                    ContractorTile.TileText = "Contractors";
                    ContractorTile.TileValue = 0;
                    ContractorTile.TileID = "contractors";
                    ContractorTile.TileToolTipText = "Total Client Users Count";
                    ContractorTile.TileArrowText = "ri-user-2-fill";
                    ContractorTile.TileSubText = "text-warning";
                    TilesList.Add(ContractorTile);
                }
                #endregion
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
                return PartialView("_GetSecondaryTiles", TilesList);
            }
            return PartialView("_GetSecondaryTiles", TilesList);
        }
        public async Task<List<Itemlist>> GetFacilityItemList()
        {
            var FacilityList = new List<Itemlist>();
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user.UserType != Convert.ToInt32(UserTypes.Thinkanew))
                {
                    var userOrg = _unitOfWork.UserOrganizationFacilitiesRepo.GetAll().Where(x => x.UserId == user.Id && x.IsActive).FirstOrDefault();

                    var facilities = _unitOfWork.FacilityRepo.GetAll().Where(x => x.OrganizationId == userOrg.OrganizationID && x.IsActive).Select(x => new Itemlist
                    {
                        Text = x.FacilityName,
                        Value = x.Id
                    }).ToList();
                    FacilityList = facilities;

                }
                else
                {
                    var facilities = _unitOfWork.FacilityRepo.GetAll().Where(x => x.IsActive).Select(x => new Itemlist
                    { Text = x.FacilityName, Value = x.Id }).ToList();
                    FacilityList = facilities;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
                return FacilityList;
            }
            return FacilityList;
        }
        public async Task<IActionResult> GetStaffingData(int orgId = 0, int facilityId = 0, int year = 0)
        {
            ChartData ct = new ChartData();
            try
            {
                var facilityList = await GetFacilitylistAsync();
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var UserOrg = new UserOrganizationFacility();
                if (user != null && user.UserType != Convert.ToInt32(UserTypes.Thinkanew))
                {
                    UserOrg = _unitOfWork.UserOrganizationFacilitiesRepo.GetAll().Where(x => x.UserId == user.Id).FirstOrDefault();
                    orgId = UserOrg != null ? UserOrg.OrganizationID : 0;
                }
                if (orgId > 0)
                {
                    var x = facilityList.Where(x => x.OrganizationId == orgId && x.IsActive).Select(x => new Itemlist { Text = x.FacilityName, Value = x.Id }).ToList();
                    ViewBag.Facility = x;
                }
                else
                {
                    var x = facilityList.Select(x => new Itemlist { Text = x.FacilityName, Value = x.Id }).ToList();
                    ViewBag.Facility = x;
                }

                var myYear = new List<Itemlist>();
                myYear.Add(new Itemlist { Text = DateTime.Now.Year.ToString(), Value = DateTime.Now.Year });
                myYear.Add(new Itemlist { Text = DateTime.Now.AddYears(-1).Year.ToString(), Value = DateTime.Now.AddYears(-1).Year });
                myYear.Add(new Itemlist { Text = DateTime.Now.AddYears(-2).Year.ToString(), Value = DateTime.Now.AddYears(-2).Year });
                ViewBag.Year = myYear;
                if (UserOrg.Id > 0 && orgId > 0)
                {
                    var organization = _unitOfWork.OrganizationRepo.GetAll().Where(x => x.IsActive && x.OrganizationID == orgId).Select(x => new Itemlist
                    {
                        Text = x.OrganizationName,
                        Value = x.OrganizationID
                    }).ToList();
                    ViewBag.Organization = organization;
                }
                else
                {
                    var organization = _unitOfWork.OrganizationRepo.GetAll().Where(x => x.IsActive).Select(x => new Itemlist
                    {
                        Text = x.OrganizationName,
                        Value = x.OrganizationID
                    }).ToList();
                    ViewBag.Organization = organization;
                }

                var emplist = new List<Itemlist>();

                var empCount = await _unitOfWork.EmployeeRepo.GetStaffingData(orgId, facilityId, year);
                ct.Labels = empCount.Select(item => item.Text).ToArray();
                ct.Data = empCount.Select(item => item.Value).ToArray();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
                return PartialView("_StaffingDataBar", ct);
            }
            return PartialView("_StaffingDataBar", ct);
        }

        //Upload Started 
        public async Task<IActionResult> GetPendingStaffingdata()
        {
            ChartData ct = new ChartData();
            try
            {
                var thisMonth = DateTime.Now.Month;
                int quarter = 0;
                int OrgId = 0;
                if (thisMonth == 1 || thisMonth == 2 || thisMonth == 3)
                {
                    quarter = 2;
                }
                else if (thisMonth == 4 || thisMonth == 5 || thisMonth == 6)
                {
                    quarter = 3;
                }
                else if (thisMonth == 7 || thisMonth == 8 || thisMonth == 9)
                {
                    quarter = 4;
                }
                else
                {
                    quarter = 1;
                }
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user != null && user.UserType != Convert.ToInt32(UserTypes.Thinkanew))
                {
                    var UserOrg = _unitOfWork.UserOrganizationFacilitiesRepo.GetAll().Where(x => x.UserId == user.Id).FirstOrDefault();
                    OrgId = UserOrg != null ? UserOrg.OrganizationID : 0;
                }

                var pendingCount = await _unitOfWork.EmployeeRepo.GetPendingStaffingdata(quarter, OrgId);
                int[] test = new int[pendingCount];
                ct.Data = test;
                ViewBag.pendingStaffingdata = pendingCount;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
            }
            return PartialView("_GetPendingStaffingdata", ct);
        }
        #region submissions
        public async Task<IActionResult> GetLastQuarterSubmitHistory()
        {
            var thisMonth = DateTime.Now.Month;
            int quarter = 0;
            int OrgId = 0;
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user != null && user.UserType != Convert.ToInt32(UserTypes.Thinkanew))
                {
                    var UserOrg = _unitOfWork.UserOrganizationFacilitiesRepo.GetAll().Where(x => x.UserId == user.Id).FirstOrDefault();
                    OrgId = UserOrg != null ? UserOrg.OrganizationID : 0;
                }
                if (thisMonth == 1 || thisMonth == 2 || thisMonth == 3)
                {
                    quarter = 1;
                }
                else if (thisMonth == 4 || thisMonth == 5 || thisMonth == 6)
                {
                    quarter = 2;
                }
                else if (thisMonth == 7 || thisMonth == 8 || thisMonth == 9)
                {
                    quarter = 3;
                }
                else
                {
                    quarter = 4;
                }
                var pendingCount = await _unitOfWork.HistoryRepo.GetLastQuarterSubmitHistory(quarter, OrgId);
                ViewBag.pendingStaffingdata = pendingCount;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
            }
            return PartialView("_GetLastQuarterSubmitHistory");
        }
        public async Task<IActionResult> GetOrgWisePendingSubmission()
        {
            ChartData ct = new ChartData();
            var thisMonth = DateTime.Now.Month;
            int quarter = 0;
            int OrgId = 0;
            try
            {
                if (thisMonth == 1 || thisMonth == 2 || thisMonth == 3)
                {
                    quarter = 4;
                }
                else if (thisMonth == 4 || thisMonth == 5 || thisMonth == 6)
                {
                    quarter = 1;
                }
                else if (thisMonth == 7 || thisMonth == 8 || thisMonth == 9)
                {
                    quarter = 2;
                }
                else
                {
                    quarter = 3;
                }
                var user = await _userManager.GetUserAsync(HttpContext.User);

                if (user != null && user.UserType != Convert.ToInt32(UserTypes.Thinkanew))
                {
                    var UserOrg = _unitOfWork.UserOrganizationFacilitiesRepo.GetAll().Where(x => x.UserId == user.Id).FirstOrDefault();
                    OrgId = UserOrg != null ? UserOrg.OrganizationID : 0;
                }
                var pendingFacilities = await _unitOfWork.HistoryRepo.GetPendingSubmit(quarter, OrgId);
                var orgs = _unitOfWork.OrganizationRepo.GetAll().Where(x => x.IsActive).ToList();
                var x = from p in pendingFacilities join o in orgs on p.OrganizationId equals o.OrganizationID select new Itemlist { Text = o.OrganizationName, Value = o.OrganizationID };
                var y = x.GroupBy(x => x.Text).Select(x => new Itemlist { Text = x.Key.ToString(), Value = x.Count() });

                var PendingOrg = pendingFacilities.GroupBy(x => x.OrganizationId).Select(g => new Itemlist { Text = g.Key.ToString(), Value = g.Count() }).ToList();
                var PendingfOrg = pendingFacilities.GroupBy(x => x.OrganizationId)
                    .Select(g => new
                    {
                        OrganizationId = g.Key,
                        Count = g.Count()
                    }).Join(orgs, grouped => grouped.OrganizationId, org => org.OrganizationID, (grouped, org) => new Itemlist

                    {
                        Text = org.OrganizationName, // Assuming the organization name property is OrganizationName
                        Value = grouped.Count
                    })
                     .ToList();



                ct.Labels = PendingfOrg.Select(item => item.Text).ToArray();
                ct.Data = PendingfOrg.Select(item => item.Value).ToArray();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
            }
            return PartialView("_GetOrgWisePendingSubmission", ct);
        }

        //Submission Status
        public async Task<IActionResult> GetOrgWiseSubmissionStatus(int OrgId = 0)
        {
            ChartData ct = new ChartData();
            if (OrgId == null)
            {
                OrgId = 0;
            }
            var thisMonth = DateTime.Now.Month;
            int quarter = 0;
            try
            {
                //pas last quarter
                if (thisMonth == 1 || thisMonth == 2 || thisMonth == 3)
                {
                    quarter = 1;
                }
                else if (thisMonth == 4 || thisMonth == 5 || thisMonth == 6)
                {
                    quarter = 2;
                }
                else if (thisMonth == 7 || thisMonth == 8 || thisMonth == 9)
                {
                    quarter = 3;
                }
                else
                {
                    quarter = 4;
                }
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var UserOrg = new UserOrganizationFacility();
                if (user != null && user.UserType != Convert.ToInt32(UserTypes.Thinkanew))
                {
                    UserOrg = _unitOfWork.UserOrganizationFacilitiesRepo.GetAll().Where(x => x.UserId == user.Id).FirstOrDefault();
                    OrgId = UserOrg != null ? UserOrg.OrganizationID : 0;
                }
                var pendingFacilities = await _unitOfWork.HistoryRepo.GetSubmitStatus(quarter, OrgId);
                var orgs = new List<OrganizationModel>();
                if (UserOrg.Id > 0 && OrgId > 0)
                {
                    orgs = _unitOfWork.OrganizationRepo.GetAll().Where(c => c.IsActive && c.OrganizationID == OrgId).ToList();
                }
                else
                {
                    orgs = _unitOfWork.OrganizationRepo.GetAll().Where(c => c.IsActive).ToList();
                }
                ViewBag.Organization = orgs.Select(x => new Itemlist { Text = x.OrganizationName, Value = x.OrganizationID }).ToList();

                if (pendingFacilities != null && pendingFacilities.Count == 0)
                {
                    int[] arr = { 0, 0, 0, 0 };
                    string[] strArr = { "Submitted", "Pending", "Accepted", "Rejected" };
                    ct.Labels = strArr;
                    ct.Data = arr;
                }

                else
                {
                    ct.Labels = pendingFacilities.Select(item => item.Text).ToArray();
                    ct.Data = pendingFacilities.Select(item => item.Value).ToArray();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
            }
            return PartialView("_GetOrgWiseSubmissionStatus", ct);
        }
        #endregion
        public IActionResult Privacy()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult EmailTemplates()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public async Task<IActionResult> Logout()
        {
            var user = await _userManager.GetUserAsync(User);
            await _signInManager.SignOutAsync();
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            var agent = UserAgentHelper.GetBrowserName(userAgent);
            var log = _unitOfWork.loginAuditRepo.GetAll().Where(x => x.UserId == user.Id && x.IPAddress == HttpContext.Connection.RemoteIpAddress?.ToString() && x.LogOutTime == null && x.UserAgent == agent).OrderByDescending(x => x.LoginTime).FirstOrDefault();

            if (log != null)
            {

                //logout.LoginAuditId = log.LoginAuditId;
                log.UserId = user.Id;
                log.LoginStatus = Convert.ToInt32(LoginAuditStatus.Success);
                log.LogOutTime = DateTime.UtcNow;
                log.IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                _unitOfWork.loginAuditRepo.Update(log);
                _unitOfWork.SaveChanges();
            }
            Log.Information("Logout", user.UserName + "logged out successfully");
            return RedirectToAction("Index");
        }
        public IActionResult Maintenance()
        {
            return View();
        }

        [Route("/maintenancepage")]
        public IActionResult MaintenancePage()
        {
            return View("~/Views/Shared/Error503.cshtml");
        }

        [Route("/lockscreen")]
        public async Task<IActionResult> LockScreen()
        {
            LockScreenViewModel lockScreenViewModel = new();
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            lockScreenViewModel.currentUrl = Request.Headers["Referer"].ToString();
            lockScreenViewModel.UserName = currentUser.FirstName + " " + currentUser.LastName;
            lockScreenViewModel.Email = currentUser.UserName;
            if (!IsSessionValid(HttpContext))
            {
                var user = await _userManager.FindByEmailAsync(currentUser.UserName);
                var defApp = _unitOfWork.ApplicationRepo.GetAll().Where(x => x.ApplicationId == (user.DefaultAppId)).FirstOrDefault();
                var pbjsnap = _configuration[TANResource.PBJSnap];
                var inventory = _configuration[TANResource.Inventory];
                if (defApp.Name == inventory)
                {
                    return Redirect(TANResource.InventoryDashboardPath);
                }
                else if (defApp.Name == pbjsnap)
                {
                    ChartData EmpPayType = await EmployeePayTypePiechart();
                    ViewBag.ChartData = EmpPayType;
                    ChartData EmpDistribution = await EmployeeDistributionBarchart();
                    ViewBag.BarChartData = EmpDistribution;
                    ChartData EmpLabourType = await EmployeeLabourTypechart();
                    ViewBag.BarChartLabourData = EmpLabourType;
                    return Redirect(TANResource.PBJDashboardPath);
                }
                else
                {
                    return Redirect(TANResource.ReportingDashboardPath);
                }
            }
            else
            {
                return View("~/Views/Shared/LockScreen.cshtml", lockScreenViewModel);
            }
        }

        [HttpPost]
        [Route("/lockscreen")]
        public async Task<IActionResult> LockScreen(LockScreenViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (IsSessionValid(HttpContext))
                {
                    var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, lockoutOnFailure: true);
                    if (result.Succeeded)
                    {
                        var user = await _userManager.FindByEmailAsync(model.Email);
                        var defApp = _unitOfWork.ApplicationRepo.GetAll().Where(x => x.ApplicationId == (user.DefaultAppId)).FirstOrDefault();
                        var pbjsnap = _configuration[TANResource.PBJSnap];
                        var inventory = _configuration[TANResource.Inventory];
                        if (defApp.Name == inventory)
                        {
                            return Redirect(TANResource.InventoryDashboardPath);
                        }
                        else if (defApp.Name == pbjsnap)
                        {
                            ChartData EmpPayType = await EmployeePayTypePiechart();
                            ViewBag.ChartData = EmpPayType;
                            ChartData EmpDistribution = await EmployeeDistributionBarchart();
                            ViewBag.BarChartData = EmpDistribution;
                            ChartData EmpLabourType = await EmployeeLabourTypechart();
                            ViewBag.BarChartLabourData = EmpLabourType;
                            return Redirect(TANResource.PBJDashboardPath);
                        }
                        else
                        {
                            return Redirect(TANResource.ReportingDashboardPath);
                        }
                    }
                }
                else
                {
                    await _signInManager.SignOutAsync();
                    return Redirect(TANResource.PBJDashboardPath);
                }
            }
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        public bool IsSessionValid(HttpContext httpContext)
        {
            var lastActivity = httpContext.Session.GetString("LastActivity");

            if (string.IsNullOrEmpty(lastActivity))
            {
                // Session doesn't exist, return false
                return false;
            }

            // Parse the timestamp
            var lastActivityTime = DateTimeOffset.Parse(lastActivity);

            // Calculate the time difference
            var timeDifference = DateTimeOffset.Now - lastActivityTime;

            // Check if the time difference is within 15 minutes
            return timeDifference.TotalMinutes <= 30;
        }

        public JsonResult CheckSessionTimeout()
        {
            bool isValidSession = IsSessionValid(HttpContext);
            return Json(isValidSession);
        }
    }
}
