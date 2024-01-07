using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using Serilog;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Web.Helpers;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.Models;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;


namespace TANWeb.Areas.TelecomReporting.Controllers
{
    [Area("TelecomReporting")]
    [Authorize]
    public class CallDetailsController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<AspNetUser> _userManager;
        public CallDetailsController(IUnitOfWork unit, UserManager<AspNetUser> userManager)
        {
            unitOfWork = unit;
            _userManager = userManager;
        }

        [Route("telecomreporting/reports/callsummaryreport")]
        [Authorize(Permissions.Reports.TelecomReportsView)]
        [Authorize(Permissions.Voice.TelecomVoiceView)]
        public async Task<IActionResult> Index()
        {
            BandwidthCallSearchViewModel callSearch = new BandwidthCallSearchViewModel();
            try
            {
                callSearch.CallDirection = Enum.GetValues(typeof(CallDirection))
                          .Cast<CallDirection>()
                          .Select(status => new SelectListText
                          {
                              Value = status.ToString(),
                              Text = EnumHelper.GetEnumDescription(status)
                          })
                          .ToList();
                callSearch.CallType = Enum.GetValues(typeof(CallType))
                              .Cast<CallType>()
                              .Select(status => new SelectListText
                              {
                                  Value = status.ToString(),
                                  Text = EnumHelper.GetEnumDescription(status)
                              })
                              .ToList();
                callSearch.CallResult = Enum.GetValues(typeof(CallResult))
                              .Cast<CallResult>()
                              .Select(status => new SelectListText
                              {
                                  Value = status.ToString(),
                                  Text = EnumHelper.GetEnumDescription(status)
                              })
                              .ToList();
                callSearch.HangUpSource = Enum.GetValues(typeof(HangUpSource))
                              .Cast<HangUpSource>()
                              .Select(status => new SelectListText
                              {
                                  Value = status.ToString(),
                                  Text = EnumHelper.GetEnumDescription(status)
                              })
                              .ToList();
                var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                callSearch.UserType = currentUser.UserType;
                callSearch.bandwidthAccessLists = await unitOfWork.OrganizationRepo.GetBandwidthAccessList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured CallDetails/Index", ex.Message);

            }
            return View(callSearch);
        }

        [HttpPost]
        [Authorize(Permissions.Reports.TelecomReportsView)]
        [Authorize(Permissions.Voice.TelecomVoiceView)]
        public JsonResult GetCallDetailsData(string? searchvalue, string? start, string? length, int draw, string? sortColumn, string? sortDirection, BandWidthCallFilterViewModel? bandWidthCallFilter)
        {
            //var draw = Request.Form["draw"].FirstOrDefault();
            IEnumerable<BandwidthCallEventResponse> bandwidthCallEventResponses = new List<BandwidthCallEventResponse>();
            int totalRecord = 0;
            try
            {
                string sortOrder = string.Empty;
                int bandwidthCallCount = 0;
                int filterRecord = 0;
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
                {
                    sortOrder = (sortColumn + "_" + sortDirection).ToLower();
                }
                bandwidthCallEventResponses = unitOfWork.BandwidthCallEventsRepo.CallDetails(searchvalue, sortOrder, start, length, out bandwidthCallCount, bandWidthCallFilter);
                totalRecord = bandwidthCallCount;
                filterRecord = bandwidthCallCount;

                var returnObj = new { draw = draw, recordsTotal = totalRecord, recordsFiltered = filterRecord, data = bandwidthCallEventResponses.ToList() };
                return Json(returnObj);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured CallDetails/GetCallDetailsData", ex.Message);

            }

            var _returnObj = new
            {
                //draw = draw,
                recordsTotal = totalRecord,
                recordsFiltered = totalRecord,
                data = bandwidthCallEventResponses
            };
            return Json(_returnObj);

        }

        [HttpPost]
        [Authorize(Permissions.Reports.TelecomReportsEdit)]
        [Authorize(Permissions.Voice.TelecomVoiceEdit)]
        public async Task<IActionResult> GetCallDetailsById(string callId)
        {
            //var draw = Request.Form["draw"].FirstOrDefault();
            BandwidthCallEventResponse bandwidthCallEventResponses = new BandwidthCallEventResponse();
            int totalRecord = 0;
            try
            {

                bandwidthCallEventResponses = await unitOfWork.BandwidthCallEventsRepo.CallDetailsById(callId);
                return Json(bandwidthCallEventResponses);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured CallDetails/GetCallDetailsData", ex.Message);

            }

            return NotFound();

        }

        [Route("telecomreporting/reports/costanalysisreport")]
        [Authorize(Permissions.Reports.TelecomReportsView)]
        [Authorize(Permissions.Voice.TelecomVoiceView)]
        public async Task<IActionResult> CostAnalysisReport()
        {
            CostAnalysisViewModel costAnalysisViewModel = new CostAnalysisViewModel();
            costAnalysisViewModel.CallDirections = Enum.GetValues(typeof(CallDirection))
                           .Cast<CallDirection>()
                           .Select(direction => new SelectListText
                           {
                               Value = direction.ToString(),
                               Text = EnumHelper.FormatEnumDescription(direction)
                           }).ToList();

            costAnalysisViewModel.CallTypes = Enum.GetValues(typeof(CallType))
                           .Cast<CallType>()
                           .Select(type => new SelectListText
                           {
                               Value = type.ToString(),
                               Text = EnumHelper.FormatEnumDescription(type)
                           }).ToList();

            costAnalysisViewModel.CallResults = Enum.GetValues(typeof(CallResult))
                           .Cast<CallResult>()
                           .Select(result => new SelectListText
                           {
                               Value = result.ToString(),
                               Text = EnumHelper.FormatEnumDescription(result)
                           }).ToList();

            var allOrganizations = unitOfWork.OrganizationRepo.GetAll().Where(o => o.IsActive).Select(o => new Itemlist { Text = o.OrganizationName, Value = o.OrganizationID }).ToList();

            bool isThinkAnewUser = unitOfWork.BandwidthCallEventsRepo.GetLoggedInUserInfo();
            if (!isThinkAnewUser)
            {
                var userOrganizationInfoDetails = unitOfWork.BandwidthCallEventsRepo.GetOrganizationUserDetails();
                if (userOrganizationInfoDetails != null)
                {
                    costAnalysisViewModel.Organizations = allOrganizations.Where(o => o.Value.Equals(Convert.ToInt32(userOrganizationInfoDetails))).ToList();
                }
            }
            else
            {

                var voiceServiceOrganizations = unitOfWork.OrganizationServiceRepo.GetAll().Where(os => os.IsActive && os.ServiceType == Convert.ToInt32(ServiceType.Voice)).ToList();

                var organizationsInVoiceService = from allOrg in allOrganizations
                                                  join voiceOrg in voiceServiceOrganizations
                                                  on allOrg.Value equals voiceOrg.OrganizationId
                                                  select allOrg;
                costAnalysisViewModel.Organizations = organizationsInVoiceService.ToList();
            }
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            costAnalysisViewModel.UserType = currentUser.UserType;
            return View(costAnalysisViewModel);
        }

        [HttpPost]
        [Authorize(Permissions.Reports.TelecomReportsView)]
        [Authorize(Permissions.Voice.TelecomVoiceView)]
        public JsonResult GetCostAnalysisReportList(string searchValue, string start, string length, string sortColumn, string sortDirection, int draw, string callDirection, string callType, string callResult, string orgName)
        {
            int totalRecord = 0;
            int filterRecord = 0;

            int pageSize = Convert.ToInt32(length ?? "0");
            int skip = Convert.ToInt32(start ?? "0");
            string sortOrder = string.Empty;

            IEnumerable<CostAnalysisViewModel> data;

            int callCount = 0;

            //sort data
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
            {
                sortOrder = (sortColumn + "_" + sortDirection).ToLower();
            }
            data = unitOfWork.BandwidthCallEventsRepo.GetAllCallsCostAnalysis(skip, pageSize, searchValue, sortOrder, callDirection, callType, callResult, orgName, out callCount);
            totalRecord = callCount;

            // get total count of records after search 
            filterRecord = callCount;

            var returnObj = new { draw = draw, recordsTotal = totalRecord, recordsFiltered = filterRecord, data = data.ToList() };
            return Json(returnObj);
        }

        [Authorize(Permissions.Voice.TelecomVoiceExport)]
        public FileResult ExportCostSummaryToCsv(CostSummaryCsvRequest costSummaryRequest)
        {
            try
            {
                StringBuilder csv = new StringBuilder();
                DataTable costSummary = unitOfWork.BandwidthCallEventsRepo.GetCostSummaryByOrganization(costSummaryRequest);
                if (costSummary.Rows.Count > 0)
                {

                    IEnumerable<string> columnNames = costSummary.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);
                    csv.AppendLine(string.Join(",", columnNames));
                    foreach (DataRow row in costSummary.Rows)
                    {
                        IEnumerable<string> fields = row.ItemArray.Select(field =>
                          string.Concat("\"", field.ToString().Replace("\"", "\"\""), "\""));
                        csv.AppendLine(string.Join(",", fields));
                    }
                    string filename = costSummaryRequest.FileName + ".csv";
                    costSummary.Clear();
                    return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", filename);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured ExportCostSummaryToCsv :{ErrorMsg}", ex.Message);

            }
            return File(Encoding.UTF8.GetBytes(""), "text/csv", costSummaryRequest.FileName + ".csv");
        }

        [Authorize(Permissions.Voice.TelecomVoiceExport)]
        public FileResult ExportCallDetails(ExportCallHistoryRequest exportCallHistoryRequest)
        {
            try
            {
                StringBuilder csv = new StringBuilder();
                DataTable CallDetails = unitOfWork.BandwidthCallEventsRepo.CallDetailsExportList(exportCallHistoryRequest);
                if (CallDetails.Rows.Count > 0)
                {
                    IEnumerable<string> columnNames = CallDetails.Columns.Cast<DataColumn>().
                                             Select(column => column.ColumnName);
                    csv.AppendLine(string.Join(",", columnNames));
                    foreach (DataRow row in CallDetails.Rows)
                    {
                        IEnumerable<string> fields = row.ItemArray.Select(field =>
                          string.Concat("\"", field.ToString().Replace("\"", "\"\""), "\""));
                        csv.AppendLine(string.Join(",", fields));
                    }
                    string filename = exportCallHistoryRequest.FileName + ".csv";
                    CallDetails.Clear();
                    return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", filename);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured ExportCensusToCsv :{ErrorMsg}", ex.Message);
            }
            return File(Encoding.UTF8.GetBytes(""), "text/csv", exportCallHistoryRequest.FileName + ".csv");
        }
    }
}
