using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Data;
using System.Text.RegularExpressions;
using System.Security.Claims;
using System.Text;
using System.Web.WebPages;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.Models;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Azure.Core.Pipeline;
using iTextSharp.text.html.simpleparser;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.pipeline.html;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.pipeline.end;
using iTextSharp.tool.xml.parser;
using Microsoft.AspNetCore.Authorization;
using TANWeb.Areas.PBJSnap.PBJSnapServices;
using iTextSharp.text.pdf;
using iTextSharp.text;
using static TAN.DomainModels.Models.XMLReaderModel;
using TANWeb.Interface;
using TANWeb.Helpers;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.DataProtection;
using System.Web.Helpers;

namespace TANWeb.Areas.PBJSnap.Controllers
{
    [Area("PBJSnap")]
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly Microsoft.AspNetCore.Identity.UserManager<AspNetUser> _UserManager;
        private readonly IPdfExportHelper _pDFExportHelper;
        private readonly IUpload _iupload;
        private readonly IDataProtector _protector;
        public ReviewController(IUnitOfWork uow, Microsoft.AspNetCore.Identity.UserManager<AspNetUser> UserManager, IPdfExportHelper pDFExportHelper, IUpload iupload, IDataProtectionProvider dataProvider)
        {
            _uow = uow;
            _UserManager = UserManager;
            _pDFExportHelper = pDFExportHelper;
            _iupload = iupload;
            _protector = dataProvider.CreateProtector("Code");
        }
        [Route("/get-started/review")]
        public async Task<IActionResult> Index(string? facilityId, string? fileId)
        {
            ReviewModel reviewModel = new ReviewModel();
            reviewModel.EmployeeDetailModel = new EmployeeModel();
            reviewModel.EmployeeDetailModel.employeePopupModel = new EmployeePopupModel();
            try
            {
                reviewModel.FacilityList = await _uow.FacilityRepo.GetAllFacilitiesByOrgId();
                reviewModel.EmployeeDetailModel.employeePopupModel.PayType = await _uow.PayTypeCodesRepo.GetAllPayTypeCodes();
                reviewModel.EmployeeDetailModel.employeePopupModel.JobCode = _uow.JobCodesRepo.GetAll().Where(x => x.IsActive).ToList();
                reviewModel.FacilityId = (facilityId != null) ? Convert.ToString(_protector.Unprotect(facilityId)) : null;
                ViewBag.FacilityId = reviewModel.FacilityId;
                reviewModel.FileId = (fileId != null) ? Convert.ToString(_protector.Unprotect(fileId)) : "";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured Review Index :{ErrorMsg}", ex.Message);
            }
            return View(reviewModel);
        }

        [EnableRateLimiting("AddPolicy")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCencus(Census cencus)
        {
            Response response = new Response();
            try
            {
                if (cencus != null)
                {
                    var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (UserId != null)
                    {
                        if (await _uow.CensusRepo.AddCencus(cencus))
                        {
                            response.StatusCode = 200;
                            response.Message = "Record saved successfully";
                            return Json(response);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured AddCencus :{ErrorMsg}", ex.Message);
            }
            response.StatusCode = 400;
            response.Message = "Record failed to insert";
            return Json(response);
        }

        [Authorize(Permissions.Review.PBJSnapDeleteCensus)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCencus(string CencusId)
        {
            Response response = new Response();
            try
            {
                if (CencusId != null)
                {
                    var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (UserId != null)
                    {
                        if (await _uow.CensusRepo.DeleteCencus(Convert.ToInt32(CencusId)))
                        {
                            response.StatusCode = 200;
                            response.Message = "Record deleted successfully";
                            return Json(response);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
            }
            response.StatusCode = 400;
            response.Message = "Record failed to delete";
            return Json(response);
        }

        [HttpPost]
        public IActionResult GetCensus(CensusViewRequest cencusViewRequest)
        {
            CensusViewModel cencusViewModel = new CensusViewModel();
            int CencusTotalCount = 0;
            var draw = Request.Form["draw"].FirstOrDefault();
            string sortOrder = string.Empty;
            try
            {
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                cencusViewRequest.SearchValue = Request.Form["search[value]"].FirstOrDefault();
                int pageSize = Convert.ToInt32(length ?? "0");
                int sortColumnIndex = Convert.ToInt32(Request.Form["order[0][column]"].FirstOrDefault());
                string sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                string sortColumnName = Request.Form[$"columns[{sortColumnIndex}][data]"];
                if (!string.IsNullOrEmpty(sortColumnName) && !string.IsNullOrEmpty(sortDirection))
                {
                    sortOrder = (sortColumnName + "_" + sortDirection).ToLower();
                }
                cencusViewModel.CensusData = _uow.CensusRepo.GetCencusList(pageSize, Convert.ToInt32(start), out CencusTotalCount, cencusViewRequest, sortOrder);
                var returnObj = new { draw = draw, recordsTotal = CencusTotalCount, recordsFiltered = CencusTotalCount, data = cencusViewModel.CensusData.ToList() };
                return Json(returnObj);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in GetCensus :{ErrorMsg}", ex.Message);
            }
            var _returnObj = new { draw = draw, recordsTotal = CencusTotalCount, recordsFiltered = CencusTotalCount, data = cencusViewModel.CensusData.ToList() };
            return Json(_returnObj);
        }

        [HttpPost]
        public IActionResult GetStaffingData(StaffingDepartmentCsvRequest csvRequest)
        {
            StaffingDepartmentModel staffingDepartmentModel = new StaffingDepartmentModel();
            int StaffDeptTotalCount = 0;
            var draw = Request.Form["draw"].FirstOrDefault();
            string sortOrder = string.Empty;
            try
            {
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                int pageSize = Convert.ToInt32(length ?? "0");
                int sortColumnIndex = Convert.ToInt32(Request.Form["order[0][column]"].FirstOrDefault());
                string sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                string sortColumnName = Request.Form[$"columns[{sortColumnIndex}][data]"];
                if (!string.IsNullOrEmpty(sortColumnName) && !string.IsNullOrEmpty(sortDirection))
                {
                    sortOrder = (sortColumnName + "_" + sortDirection).ToLower();
                }

                staffingDepartmentModel.SatffingDepartmentList = _uow.TimesheetRepo.GetStaffingDepartmentDetails(csvRequest, pageSize, Convert.ToInt32(start), out StaffDeptTotalCount, searchValue, sortOrder);

                var returnObj = new { draw = draw, recordsTotal = StaffDeptTotalCount, recordsFiltered = StaffDeptTotalCount, data = staffingDepartmentModel.SatffingDepartmentList.ToList() };
                return Json(returnObj);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
            }
            var _returnObj = new { draw = draw, recordsTotal = StaffDeptTotalCount, recordsFiltered = StaffDeptTotalCount, data = staffingDepartmentModel.SatffingDepartmentList.ToList() };
            return Json(_returnObj);
        }
        [HttpPost]
        public IActionResult EmployeeData(EmployeeListRequest employeeList)
        {
            EmployeeModel employeeModel = new EmployeeModel();
            int EmployeeCount = 0;
            var draw = Request.Form["draw"].FirstOrDefault();
            string sortOrder = string.Empty;
            try
            {
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                int pageSize = Convert.ToInt32(length ?? "0");
                int sortColumnIndex = Convert.ToInt32(Request.Form["order[0][column]"].FirstOrDefault());
                string sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                string sortColumnName = Request.Form[$"columns[{sortColumnIndex}][data]"];
                if (!string.IsNullOrEmpty(sortColumnName) && !string.IsNullOrEmpty(sortDirection))
                {
                    sortOrder = (sortColumnName + "_" + sortDirection).ToLower();
                }
                employeeModel.EmployeeDataList = _uow.EmployeeRepo.GetActiveEmployeesData(pageSize, Convert.ToInt32(start), employeeList, out EmployeeCount, searchValue, sortOrder);

                var returnObj = new
                {
                    draw = draw,
                    recordsTotal = EmployeeCount,
                    recordsFiltered = EmployeeCount,
                    data = employeeModel.EmployeeDataList.ToList()
                };
                return Json(returnObj);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in EmployeeData:{ErrorMsg}", ex.Message);
            }
            var _returnObj = new
            {
                draw = draw,
                recordsTotal = EmployeeCount,
                recordsFiltered = EmployeeCount,
                data = employeeModel.EmployeeDataList.ToList()
            };
            return Json(_returnObj);
        }

        //[RateLimit(requestCount: 3, timeInMinutes: (int)TimeInMinutes.Minute)]
        //[EnableRateLimiting("AddPolicy")]
        [EnableRateLimiting("AddPolicy")]
        [ValidateAntiForgeryToken]
        public IActionResult SaveEmployeeeData(EmployeeData employeeData)
        {
            try
            {
                if (employeeData != null)
                {
                    Employee employee = new Employee();
                    var errorMassage = "";
                    employee.EmployeeId = employeeData.EmployeeId;
                    if (employeeData.EmployeeId?.Trim().Length > 30)
                    {
                        return Ok("Employee Id must be smaller then 30 characters");
                    }
                    if (employeeData.EmployeeId!=null && !Regex.IsMatch(employeeData.EmployeeId, "^[a-zA-Z0-9]+$"))
                    {
                        errorMassage="IdError";
                        return Ok(errorMassage);
                    }
                    if (employeeData.FirstName!=null && !Regex.IsMatch(employeeData.FirstName, "^[a-zA-Z'. ]+$"))
                    {
                        errorMassage="FirstNameError";
                        return Ok(errorMassage);
                    }
                    if (employeeData.LastName!=null && !Regex.IsMatch(employeeData.LastName, "^[a-zA-Z'. ]+$"))
                    {
                        errorMassage="LastNameError";
                        return Ok(errorMassage);
                    }

                    employee.FirstName = employeeData.FirstName;
                    employee.LastName = employeeData.LastName;
                    employee.FacilityId = Convert.ToInt32(employeeData.FacilityId);
                    employee.HireDate = Convert.ToDateTime(employeeData.HireDate);
                    employee.TerminationDate = Convert.ToDateTime(employeeData.TerminationDate);
                    employee.PayTypeCode = Convert.ToInt32(employeeData.PayType);
                    employee.JobTitleCode = Convert.ToInt32(employeeData.JobTitle);
                    employee.CreateDate = DateTime.UtcNow;
                    employee.Id = employeeData.Id;
                    if (employeeData.Id == 0)
                    {
                        if (_uow.EmployeeRepo.GetAll().Where(x => x.FacilityId == Convert.ToInt32(employeeData.FacilityId)
                    && x.EmployeeId?.ToLower() == employeeData.EmployeeId?.Trim().ToLower()).Any())
                        {
                            return Ok("The employee id is already exist in the system.");
                        }
                        employee.IsActive = true;
                        employee.CreateDate = DateTime.Now;
                        employee.CreateBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        var result = _uow.EmployeeRepo.AddEmployee(employee).Result;
                        if (result)
                            return Ok("Success");
                    }
                    else
                    {
                        employee.IsActive = true;
                        employee.UpdateDate = DateTime.Now;
                        employee.UpdateBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        var result = _uow.EmployeeRepo.AddEmployee(employee).Result;
                        if (result)
                            return Ok("Success");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in SaveEmployeeeData:{ErrorMsg}", ex.Message);
            }
            return StatusCode(500);
        }
        [Authorize(Permissions.Review.PBJSnapEditCensus)]
        [HttpPost]
        public IActionResult EditCencus(string CencusId)
        {
            try
            {
                if (CencusId != "" && CencusId != null)
                {
                    var data = _uow.CensusRepo.GetAll().Where(x => x.CensusId == Convert.ToInt32(CencusId)).FirstOrDefault();
                    return Json(data);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in EmployeeData:{ErrorMsg}", ex.Message);
            }
            return NotFound();
        }

        [Authorize(Permissions.EmployeeMaster.PBJSnapEdit)]
        [HttpPost]
        public IActionResult EditEmployeeData(string Id)
        {
            try
            {
                var data = _uow.EmployeeRepo.GetAll().FirstOrDefault(x => x.Id == Convert.ToInt32(Id));
                return Json(data);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in EditEmployeeData:{ErrorMsg}", ex.Message);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> GetEmployeeTimesheet(EmployeeTimesheetDataRequest timesheetDataRequest)
        {
            List<EmployeeTimesheetData> events = new List<EmployeeTimesheetData>();
            try
            {
                events = await _uow.TimesheetRepo.GetEmployeeTimesheetData(timesheetDataRequest);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
            }
            return Json(events);
        }

        [HttpPost]
        public async Task<IActionResult> SaveEmployeeWorkingDetails(EmployeeTimesheetSaveDataRequest request)
        {
            try
            {
                if (request != null)
                {
                    Timesheet timesheet = new Timesheet();
                    timesheet.EmployeeId = request.EmployeeId;
                    timesheet.THours = Convert.ToDecimal(request.TotalHours);
                    timesheet.Workday = Convert.ToDateTime(request.WorkDate);
                    timesheet.FacilityId = request.FacilityId;
                    timesheet.PayTypeCode = request.PayCode;
                    timesheet.JobTitleCode = request.JobTitle;
                    timesheet.UploadType = request.UploadType;
                    timesheet.Year = timesheet.Workday.Year;
                    timesheet.ReportQuarter = _iupload.GetQuarter(timesheet.Workday);
                    timesheet.Month = timesheet.Workday.Month;
                    decimal totalTHours = 0;
                    if (Convert.ToInt32(request.TimesheetId) == 0)
                    {
                        totalTHours = _uow.TimesheetRepo.GetAll().Where(t => t.EmployeeId == request.EmployeeId && t.IsActive
                        && t.Workday == timesheet.Workday && t.FacilityId == request.FacilityId).Sum(t => t.THours);

                        totalTHours = totalTHours + timesheet.THours;
                        if (totalTHours > 24)
                            return Json("24hours");

                        timesheet.Createby = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        timesheet.CreateDate = DateTime.Now;
                        timesheet.IsActive = true;
                        timesheet.Status = 0;
                        if (await _uow.TimesheetRepo.AddTimesheet(timesheet))
                        {
                            return Json(200);
                        }
                    }
                    else if (Convert.ToInt32(request.TimesheetId) != 0)
                    {
                        timesheet.Id = Convert.ToInt32(request.TimesheetId);
                        timesheet.UpdateBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        timesheet.UpdateDate = DateTime.Now;
                        timesheet.IsActive = true;
                        timesheet.Status = 0;

                        totalTHours = _uow.TimesheetRepo.GetAll().Where(t => t.EmployeeId == request.EmployeeId && t.IsActive
                        && t.Workday == timesheet.Workday && t.Id != timesheet.Id && t.FacilityId == request.FacilityId)
                            .Sum(t => t.THours);
                        totalTHours = totalTHours + timesheet.THours;

                        if (totalTHours > 24)
                            return Json("24hours");

                        if (await _uow.TimesheetRepo.AddTimesheet(timesheet))
                        {
                            return Json(200);
                        }
                    }
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> GetEmployeeData(EmployeeTimesheetDataRequest timesheetDataRequest)
        {
            Timesheet events = new Timesheet();
            try
            {
                events = await _uow.TimesheetRepo.GetEmpTimesheetData(timesheetDataRequest);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
            }
            return Json(events);
        }

        [HttpPost]
        public async Task<IActionResult> EmployeeMultiDatesSave(EmployeeMultiDatesSaveRequest request)
        {
            try
            {
                if (request != null)
                {
                    if (request.MultipleDates?.Count > 0)
                    {
                        List<Timesheet> timesheetsList = new List<Timesheet>();
                        string dateList = "";
                        DateTime dateTime = DateTime.Now;
                        string UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        EmployeeTimesheetDataRequest timesheetDataRequest = new EmployeeTimesheetDataRequest();
                        timesheetDataRequest.ReportQuarter = request.ReportQuarter;
                        timesheetDataRequest.EmployeeId = request.EmployeeId;
                        timesheetDataRequest.Year = request.Year;
                        timesheetDataRequest.FacilityId = request.FacilityId;
                        foreach (var item in request.MultipleDates)
                        {
                            decimal totalTHours = 0;
                            Timesheet timesheet = new Timesheet();
                            timesheet.THours = Convert.ToDecimal(request.WorkingHours);
                            timesheet.Workday = Convert.ToDateTime(item);
                            totalTHours = _uow.TimesheetRepo.GetAll().Where(t => t.EmployeeId == request.EmployeeId && t.IsActive && t.Workday == timesheet.Workday && t.FacilityId == request.FacilityId)
                                .Sum(t => t.THours);

                            totalTHours = totalTHours + timesheet.THours;
                            if (totalTHours > 24)
                            {
                                dateList += " " + item + ",";
                                continue;
                            }

                            timesheet.EmployeeId = request.EmployeeId;
                            timesheet.FacilityId = request.FacilityId;
                            timesheet.ReportQuarter = request.ReportQuarter;
                            timesheet.Year = request.Year;
                            timesheet.Month = request.Month;
                            timesheet.Createby = UserId;
                            timesheet.CreateDate = dateTime;
                            timesheet.JobTitleCode = request.JobTitle;
                            timesheet.PayTypeCode = request.PayCode;
                            timesheet.IsActive = true;
                            timesheet.Status = 0;
                            timesheetsList.Add(timesheet);
                        }
                        if (timesheetsList.Count > 0)
                        {
                            await _uow.TimesheetRepo.InsertTimesheet(timesheetsList);
                            return Ok(dateList.Trim(','));
                        }
                        return Ok(dateList.Trim(','));
                    }
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Review Controller / EmployeeMultiDatesSave:{ErrorMsg}", ex.Message);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMultipleWorkingHoursDates(EmployeeMultiDatesSaveRequest request)
        {

            try
            {
                if (request != null && request.MultipleDates?.Count > 0)
                {
                    DateTime dateTime = DateTime.Now;
                    string UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    EmployeeTimesheetDataRequest timesheetDataRequest = new EmployeeTimesheetDataRequest();
                    timesheetDataRequest.ReportQuarter = request.ReportQuarter;
                    timesheetDataRequest.EmployeeId = request.EmployeeId;
                    timesheetDataRequest.Year = request.Year;
                    timesheetDataRequest.FacilityId = request.FacilityId;
                    foreach (var item in request.MultipleDates)
                    {
                        timesheetDataRequest.Workday = item;
                        var WorkingHoursData = await _uow.TimesheetRepo.GetEmpTimesheetDataList(timesheetDataRequest);
                        if (WorkingHoursData != null)
                        {
                            foreach (var time in WorkingHoursData)
                            {
                                time.UpdateDate = dateTime;
                                time.UpdateBy = UserId;
                                time.IsActive = false;
                                await _uow.TimesheetRepo.AddTimesheet(time);
                            }
                        }
                    }
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> ApproveUploadData(ApproveTimesheetRequest approve)
        {
            ApproveTimesheetResponse response = new ApproveTimesheetResponse();
            try
            {
                if (approve != null)
                {
                    approve.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!approve.IsApproved)
                    {
                        if (await _uow.TimesheetRepo.CheckAllValidatedTimesheet(approve))
                        {
                            response.Success = true;
                            response.Message = "NotValidated";
                            return Json(response);
                        }
                    }
                    response = await _uow.TimesheetRepo.ApproveTimesheetData(approve);
                    if (response != null)
                    {
                        return Json(response);
                    }
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured ApproveUploadData :{ErrorMsg}", ex.Message);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> AddMissingNurseHoursData(EmployeeTimesheetSaveDataRequest request)
        {
            try
            {
                if (request != null)
                {
                    List<Timesheet> timesheetList = new List<Timesheet>();
                    var result = _uow.EmployeeRepo.GetAll().Where(x => x.FacilityId == request.FacilityId && x.IsActive
                    && x.Id == Convert.ToInt32(request.EmployeeId)).FirstOrDefault();
                    if (result == null)
                    {
                        return NotFound("EmployeeNotFound");
                    }
                    if (request.WorkDateList?.Count > 0)
                    {
                        foreach (var item in request.WorkDateList)
                        {
                            Timesheet timesheet = new Timesheet();
                            timesheet.EmployeeId = result.EmployeeId;
                            timesheet.Workday = Convert.ToDateTime(item);
                            timesheet.THours = Convert.ToDecimal(request.TotalHours);
                            timesheet.FacilityId = request.FacilityId;
                            timesheet.PayTypeCode = request.PayCode;
                            timesheet.JobTitleCode = request.JobTitle;
                            timesheet.UploadType = request.UploadType;
                            timesheet.Year = timesheet.Workday.Year;
                            timesheet.ReportQuarter = _iupload.GetQuarter(timesheet.Workday);
                            timesheet.Month = timesheet.Workday.Month;
                            timesheet.Createby = User.FindFirstValue(ClaimTypes.NameIdentifier);
                            timesheet.CreateDate = DateTime.Now;
                            timesheet.IsActive = true;
                            timesheet.Status = 0;
                            timesheetList.Add(timesheet);
                        }
                        if (timesheetList.Count > 0)
                        {
                            if (await _uow.TimesheetRepo.InsertTimesheet(timesheetList))
                            {
                                return Ok();
                            }
                        }
                    }
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured AddMissingNurseHoursData :{ErrorMsg}", ex.Message);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> GetMissingNursesDates(NursesMissingDates missingDates)
        {
            List<NursesMissingDatesResponse> list = new List<NursesMissingDatesResponse>();
            try
            {
                if (missingDates != null)
                {
                    /*GenerateDatesByYearQuarterMonth byQuarter = new GenerateDatesByYearQuarterMonth(missingDates.ReportQuarter, missingDates.Month, missingDates.Year);*/
                    var DynamicDates = GenerateDynamicDate.GenerateDynamicDates(missingDates.StartDate, missingDates.EndDate);
                    var workdays = await _uow.TimesheetRepo.GetMissingDatesByJobTitle(missingDates.FacilityId, missingDates.StartDate, missingDates.EndDate, Convert.ToInt32(missingDates.JobTitle));

                    var result = DynamicDates.Where(d => !workdays.Contains(d.myDate)).Select(d => d.myDate);
                    if (result.Any())
                    {
                        foreach (var date in result)
                        {
                            NursesMissingDatesResponse response = new NursesMissingDatesResponse();
                            response.MissingDate = date;
                            response.IsEditable = true;
                            list.Add(response);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetMissingNursesDates :{ErrorMsg}", ex.Message);
            }
            return Json(list);
        }

        public FileResult ExportCensusToCsv(CensusCsvRequest cencus)
        {
            try
            {
                StringBuilder csv = new StringBuilder();
                DataTable census = _uow.CensusRepo.GetCencusByFacilityAndQuarter(cencus.FacilityID, cencus.ReportQuarter, cencus.Year);
                if (census.Rows.Count > 0)
                {

                    IEnumerable<string> columnNames = census.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);
                    csv.AppendLine(string.Join(",", columnNames));
                    foreach (DataRow row in census.Rows)
                    {
                        IEnumerable<string> fields = row.ItemArray.Select(field =>
                          string.Concat("\"", field.ToString().Replace("\"", "\"\""), "\""));
                        csv.AppendLine(string.Join(",", fields));
                    }
                    string filename = cencus.FileName + ".csv";
                    census.Clear();
                    return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", filename);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured ExportCensusToCsv :{ErrorMsg}", ex.Message);

            }
            return File(Encoding.UTF8.GetBytes(""), "text/csv", cencus.FileName + ".csv");
        }

        public FileResult ExportStaffingDeptToCsv(StaffingDepartmentCsvRequest csvRequest)
        {
            try
            {
                StringBuilder csv = new StringBuilder();
                DataTable staffDetails = _uow.TimesheetRepo.GetStaffingDepartmentDetailsForCsv(csvRequest.FacilityID, csvRequest.ReportQuarter, csvRequest.Year);
                if (staffDetails.Rows.Count > 0)
                {

                    IEnumerable<string> columnNames = staffDetails.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);
                    csv.AppendLine(string.Join(",", columnNames));
                    foreach (DataRow row in staffDetails.Rows)
                    {
                        IEnumerable<string> fields = row.ItemArray.Select(field =>
                          string.Concat("\"", field.ToString().Replace("\"", "\"\""), "\""));
                        csv.AppendLine(string.Join(",", fields));
                    }
                    string filename = csvRequest.FileName;
                    staffDetails.Clear();
                    return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", filename);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured ExportStaffingDeptToCsv :{ErrorMsg}", ex.Message);
            }
            return File(Encoding.UTF8.GetBytes(""), "text/csv", csvRequest.FileName + ".csv");
        }

        [HttpPost]
        public async Task<IActionResult> GetEmployeeIdAndName(string term, string FacilityId)
        {
            List<SearchResponse> values = new List<SearchResponse>();
            try
            {
                if (term != null)
                {
                    values = await _uow.EmployeeRepo.GetEmployeeIdAndName(term, Convert.ToInt32(FacilityId));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetEmployeeIdAndName :{ErrorMsg}", ex.Message);
            }
            return Ok(new { items = values });
        }


        public JsonResult GetEmployeeName(string employeeId)
        {
            try
            {
                var empData = _uow.EmployeeRepo.GetAll().Where(emp => emp.EmployeeId == employeeId).FirstOrDefault();
                if (empData != null)
                {

                    return Json(empData.FirstName + " " + empData.LastName);
                }
                else
                {
                    return Json(Empty);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetEmployeeName :{ErrorMsg}", ex.Message);
                return Json(Empty);
            }
        }


        public JsonResult GetEmployeeDetails(string employeeId, int facilityId)
        {
            //EmployeeById empData = new EmployeeById();
            try
            {
                if (employeeId != null)
                {
                    var empData = _uow.EmployeeRepo.GetEmployeeData(employeeId, facilityId);
                    return Json(empData);
                }
                else
                {
                    return Json(Empty);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetEmployeeDetails :{ErrorMsg}", ex.Message);
                return Json(Empty);
            }
        }


        [HttpPost]
        public async Task<JsonResult> CheckIfValidated(int facilityId, int reportQuarter, int year)
        {
            try
            {
                var reportVerificationInfo = await _uow.TimesheetRepo.GetReportValidationStatus(facilityId, reportQuarter, year);

                if (reportVerificationInfo.ValidatedBy != null && reportVerificationInfo.ValidationDate != null)
                {
                    var validationResult = new ValidationResult
                    {
                        Success = true,
                        FullName = $"Last Validated by: {reportVerificationInfo.ValidatedBy}",
                        ValidationDate = $"Last Validation Date & Time: {reportVerificationInfo.ValidationDate}"
                    };
                    return Json(validationResult);
                }
                else
                {
                    var validationResult = new ValidationResult
                    {
                        Success = false,
                    };
                    return Json(validationResult);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured CheckIfValidated :{ErrorMsg}", ex.Message);
                var validationResult = new ValidationResult
                {
                    Success = false,
                    Message = "Unable to Validate.  A message has been sent to ThinkAnew on your behalf."
                };
                return Json(validationResult);
            }
        }

        [HttpPost]
        public async Task<JsonResult> CheckIfApproved(int facilityId, int reportQuarter, int year)
        {
            try
            {
                var reportApprovedInfo = await _uow.TimesheetRepo.GetReportApprovedStatus(facilityId, reportQuarter, year);

                if (reportApprovedInfo.ApprovedBy != null && reportApprovedInfo.ApprovedDate != null)
                {
                    var approvedResult = new ValidationResult
                    {
                        Success = true,
                        FullName = $"Last Approved by: {reportApprovedInfo.ApprovedBy}",
                        ApprovedDate = $"Last Approved Date & Time: {reportApprovedInfo.ApprovedDate}"
                    };
                    return Json(approvedResult);
                }
                else
                {
                    var approvedResult = new ValidationResult
                    {
                        Success = false,
                    };
                    return Json(approvedResult);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured CheckIfApproved :{ErrorMsg}", ex.Message);
                var approvedResult = new ValidationResult
                {
                    Success = false,
                    Message = "Unable to Approve.  A message has been sent to ThinkAnew on your behalf."
                };
                return Json(approvedResult);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ValidateData(ApproveTimesheetRequest approve)
        {
            try
            {
                approve.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                ApproveTimesheetResponse response = await _uow.TimesheetRepo.ValidateTimesheetData(approve);
                if (response != null)
                {
                    return Json(response);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in ValidateData :{ErrorMsg}", ex.Message);
            }
            return NotFound();
        }

        [Authorize(Permissions.EmployeeMaster.PBJSnapDelete)]
        [HttpPost]
        public IActionResult UpdateEmployeeStatus(string employeeId, bool status)
        {
            Response response = new Response();
            try
            {
                if (employeeId != null)
                {
                    var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (UserId != null)
                    {
                        if (_uow.EmployeeRepo.UpdateEmployeeStatus(Convert.ToInt32(employeeId), status))
                        {
                            response.StatusCode = 200;
                            response.Message = "Employee Status is Updated Successfully";
                            return Json(response);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
            }
            response.StatusCode = 400;
            response.Message = "Failed to Update Employee Status";
            return Json(response);
        }

        [HttpGet]
        public async Task<IActionResult> ExportStaff(EmployeeExportRequest request)
        {
            try
            {
                if (request != null)
                {
                    GenerateDatesByYearQuarterMonth byQuarter = new GenerateDatesByYearQuarterMonth(Convert.ToInt32(request.Quarter), Convert.ToInt32(request.Month), Convert.ToInt32(request.Year));
                    request.StartDate = byQuarter.StartDate;
                    request.EndDate = byQuarter.EndDate;
                    StaffExportList staffExportList = new StaffExportList();

                    staffExportList = await _uow.TimesheetRepo.GetStaffExportData(request);
                    if (staffExportList != null)
                    {
                        if (request.FormatType == "1")
                        {
                            //CSV
                            StringBuilder csv = new StringBuilder();
                            List<string> headerRow = new List<string>
{
    "Employee Id", "First Name", "Last Name", "Hire Date", "Termination Date", "Pay Type Description", "Job Title", "Workday", "Total Hours", "Hours Pay Type",
                                "Hours Job Title"
};
                            IEnumerable<string> headerFields = headerRow.Select(field =>
                                string.Concat("\"", field.Replace("\"", "\"\""), "\""));
                            csv.AppendLine(string.Join(",", headerFields));

                            foreach (StaffExport rowData in staffExportList.staffExports)
                            {
                                List<string> fields = new List<string>
                                {
                                    string.Join(",", rowData.EmployeeId),
                                    string.Join(",", rowData.FirstName),
                                    rowData.LastName,
                                    rowData.HireDate,
                                    rowData.TerminationDate,
                                    rowData.PayTypeDescription,
                                    rowData.JobTitle,
                                    rowData.Workday,
                                    rowData.THours,
                                    rowData.HoursPayType,
                                    rowData.HoursJobTitle,
                                };
                                csv.AppendLine(string.Join(",", fields));
                            }
                            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", request.Filename + ".csv");
                        }
                        else
                        {
                            var viewName = "StaffExportPDF";
                            string html = await _pDFExportHelper.RenderToStringAsync(viewName, staffExportList);
                            var output = new MemoryStream();
                            var document = new Document(PageSize.A4);
                            document.SetMargins(10, 0, 50, 36);
                            var writer = PdfWriter.GetInstance(document, output);
                            writer.CloseStream = false;
                            document.Open();
                            var htmlContext = new HtmlPipelineContext(null);
                            htmlContext.SetTagFactory(iTextSharp.tool.xml.html.Tags.GetHtmlTagProcessorFactory());
                            ICSSResolver cssResolver = XMLWorkerHelper.GetInstance().GetDefaultCssResolver(true);
                            var pipeline = new CssResolverPipeline(cssResolver, new HtmlPipeline(htmlContext, new PdfWriterPipeline(document, writer)));
                            var worker = new XMLWorker(pipeline, true);
                            var stringReader = new StringReader(html);
                            var xmlParser = new XMLParser(worker);
                            xmlParser.Parse(stringReader);
                            document.Close();
                            writer.Close();
                            stringReader.Close();
                            worker.Close();
                            output.Position = 0;

                            var result = new FileStreamResult(new MemoryStream(output.ToArray()), "application/pdf");
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured ExportStaff :{ErrorMsg}", ex.Message);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<JsonResult> GetStatusValidatedList(int facilityId, int reportQuarter, int year)
        {
            List<ValidationResult> result = new List<ValidationResult>();
            try
            {
                var reportVerificationInfo = await _uow.TimesheetRepo.GetAllReportValidationStatus(facilityId, reportQuarter, year);

                if (reportVerificationInfo != null)
                {
                    foreach (var reportVerification in reportVerificationInfo)
                    {
                        ValidationResult validationResult = new ValidationResult();
                        validationResult.Success = true;
                        validationResult.FullName = reportVerification.ValidatedBy;
                        validationResult.ValidationDate = reportVerification.ValidationDate;
                        result.Add(validationResult);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetStatusValidatedList :{ErrorMsg}", ex.Message);
            }
            return Json(result);
        }

        [HttpPost]
        public async Task<JsonResult> GetStatusApprovedList(int facilityId, int reportQuarter, int year)
        {
            List<ValidationResult> result = new List<ValidationResult>();
            try
            {
                var reportApprovedinfo = await _uow.TimesheetRepo.GetAllReportApprovedStatus(facilityId, reportQuarter, year);

                if (reportApprovedinfo != null)
                {
                    foreach (var reportApproved in reportApprovedinfo)
                    {
                        ValidationResult validationResult = new ValidationResult();
                        validationResult.Success = true;
                        validationResult.FullName = reportApproved.ApprovedBy;
                        validationResult.ApprovedDate = reportApproved.ApprovedDate;
                        result.Add(validationResult);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetStatusApprovedList :{ErrorMsg}", ex.Message);
            }
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTimesheetData(DeleteTimesheetDataRequest request)
        {
            try
            {
                if (request != null)
                {
                    request.UserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    bool result = await _uow.TimesheetRepo.DeleteTimesheetRecord(request);
                    if (result)
                    {
                        return Ok();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured DeleteTimesheetData :{ErrorMsg}", ex.Message);
            }
            return NotFound();
        }


        public JsonResult GetSearchedEmployees(EmployeeListRequest employeeListRequest)
        {
            EmployeeModel employeeModel = new EmployeeModel();
            try
            {
                employeeModel.CustomEmployeeDataList = _uow.EmployeeRepo.GetSearchedEmployeesData(employeeListRequest);

                if (employeeModel.CustomEmployeeDataList != null)
                {
                    return Json(employeeModel.CustomEmployeeDataList);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Review Controller / GetSearchedEmployees  while getting employees :{ErrorMsg}", ex.Message);
            }
            return Json(employeeModel.CustomEmployeeDataList);
        }
    }
}
