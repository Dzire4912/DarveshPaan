using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TAN.ApplicationCore;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.Models;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static TAN.DomainModels.Helpers.Permissions;
using System.Data;
using Microsoft.VisualBasic;
using System.Dynamic;
using System.Numerics;
using System.Security.Principal;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.CodeAnalysis;

namespace TAN.Repository.Implementations
{
    [ExcludeFromCodeCoverage]
    public class CallsRepository : Repository<BandwidthCallEvents>, ICalls
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly Microsoft.AspNetCore.Identity.UserManager<AspNetUser> _userManager;
        private DatabaseContext Context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public CallsRepository(DbContext db, IHttpContextAccessor httpContext, UserManager<AspNetUser> userManager)
        {
            this.db = db;
            _httpContext = httpContext;
            _userManager = userManager;
        }

        public async Task<bool> AddCalls(List<BandwidthCallEvents> calls)
        {
            try
            {
                if (calls != null && calls.Count > 0)
                {
                    await Context.BandwidthCallEvents.AddRangeAsync(calls);
                    await Context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured AddCalls :{ErrorMsg}", ex.Message);
            }
            return false;
        }

        public async Task<List<string>> CheckDuplicateCallsId(List<string> callIds)
        {
            List<string> callId = new List<string>();
            try
            {
                if (callIds != null && callIds.Count > 0)
                {
                    callId = await Context.BandwidthCallEvents.Where(call => callIds.Contains(call.CallId)).Select(call => call.CallId)
                        .Distinct().ToListAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured CheckDuplicateCallsId :{ErrorMsg}", ex.Message);
            }
            return callId;
        }

        public IEnumerable<BandwidthCallEventResponse> CallDetails(string? searchValue, string sortOrder, string skip, string take, out int bandwidthCallCount, BandWidthCallFilterViewModel? bandWidthCallFilter)
        {
            IEnumerable<BandwidthCallEventResponse> bandwidthCallEventResponses = null;
            bandwidthCallCount = 0;
            try
            {
                var lowercaseSearchValue = searchValue?.ToLower();

                bandwidthCallEventResponses = (from bce in Context.BandwidthCallEvents
                                               join os in Context.OrganizationServices on bce.SubAccount equals os.SubAccountId
                                               join o in Context.Organizations on os.OrganizationId equals o.OrganizationID
                                               join bm in Context.BandwidthMaster on bce.AccountId equals bm.AccountId
                                               where os.ServiceType == Convert.ToInt32(ServiceType.Voice) &&
                                               (string.IsNullOrEmpty(lowercaseSearchValue) ||
                                               bm.AccountName.ToLower().Contains(lowercaseSearchValue) || o.OrganizationName.ToLower().Contains(lowercaseSearchValue) || bce.StartTime.ToString().Contains(lowercaseSearchValue)
                                               || bce.EndTime.ToString().Contains(lowercaseSearchValue) || bce.CallingNumber.ToLower().Contains(lowercaseSearchValue)
                                               || bce.CalledNumber.ToLower().Contains(lowercaseSearchValue) || bce.CallDirection.ToLower().Contains(lowercaseSearchValue)
                                               || bce.CallType.ToLower().Contains(lowercaseSearchValue) || bce.CallResult.ToLower().Contains(lowercaseSearchValue) || bce.Duration.ToString().Contains(lowercaseSearchValue))
                                               orderby bce.Id

                                               select new BandwidthCallEventResponse
                                               {
                                                   OrganizationName = o.OrganizationName,
                                                   CallId = bce.CallId,
                                                   SubAccountId = bce.SubAccount,
                                                   AccountId = bm.AccountName,
                                                   StartTime = bce.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                                   EndTime = bce.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                                   Duration = (bce.Duration.HasValue && bce.Duration != 0) ? FormatDuration(Convert.ToInt32(bce.Duration)) : null,
                                                   CallingNumber = bce.CallingNumber,
                                                   CalledNumber = bce.CalledNumber,
                                                   HangUpSource = bce.HangUpSource,
                                                   CallDirection = char.ToUpper(bce.CallDirection[0]) + bce.CallDirection.Substring(1).ToLower(),
                                                   CallType = char.ToUpper(bce.CallType[0]) + bce.CallType.Substring(1).ToLower(),
                                                   CallResult = (char.ToUpper(bce.CallResult[0]) + bce.CallResult.Substring(1).ToLower()).ToString(),
                                               }).AsEnumerable();

                if (bandwidthCallEventResponses != null && bandwidthCallEventResponses.Count() > 0)
                {

                    if (!string.IsNullOrEmpty(bandWidthCallFilter.OrganisationName))
                    {
                        bandwidthCallEventResponses = bandwidthCallEventResponses.Where(o => o.SubAccountId.ToLower().Contains(bandWidthCallFilter.OrganisationName.ToLower())).AsEnumerable();
                    }
                    if (!string.IsNullOrEmpty(bandWidthCallFilter.CallDirection))
                    {
                        bandwidthCallEventResponses = bandwidthCallEventResponses.Where(o => o.CallDirection.ToLower().Contains(bandWidthCallFilter.CallDirection.ToLower())).AsEnumerable();
                    }
                    if (!string.IsNullOrEmpty(bandWidthCallFilter.CallType))
                    {
                        bandwidthCallEventResponses = bandwidthCallEventResponses.Where(o => o.CallType.ToLower().Contains(bandWidthCallFilter.CallType.ToLower())).AsEnumerable();
                    }
                    if (!string.IsNullOrEmpty(bandWidthCallFilter.CallResult))
                    {
                        bandwidthCallEventResponses = bandwidthCallEventResponses.Where(o => o.CallResult.ToLower().Contains(bandWidthCallFilter.CallResult.ToLower())).AsEnumerable();
                    }
                    if (!string.IsNullOrEmpty(bandWidthCallFilter.HangUpSource))
                    {
                        bandwidthCallEventResponses = bandwidthCallEventResponses.Where(o => o.HangUpSource.ToLower().Contains(bandWidthCallFilter.HangUpSource.ToLower())).AsEnumerable();
                    }
                    bandwidthCallCount = bandwidthCallEventResponses.Count();
                    bandwidthCallEventResponses = bandwidthCallEventResponses.Skip(Convert.ToInt32(skip)).Take(Convert.ToInt32(take));

                    if (!string.IsNullOrEmpty(sortOrder))
                    {
                        switch (sortOrder)
                        {
                            case "accountid_asc":
                                bandwidthCallEventResponses = bandwidthCallEventResponses.OrderBy(x => x.AccountId);
                                break;
                            case "accountid_desc":
                                bandwidthCallEventResponses = bandwidthCallEventResponses.OrderByDescending(x => x.AccountId);
                                break;
                            case "organizationname_asc":
                                bandwidthCallEventResponses = bandwidthCallEventResponses.OrderBy(x => x.SubAccountId);
                                break;
                            case "organizationname_desc":
                                bandwidthCallEventResponses = bandwidthCallEventResponses.OrderByDescending(x => x.SubAccountId);
                                break;
                            case "starttime_asc":
                                bandwidthCallEventResponses = bandwidthCallEventResponses.OrderBy(x => x.StartTime);
                                break;
                            case "starttime_desc":
                                bandwidthCallEventResponses = bandwidthCallEventResponses.OrderByDescending(x => x.StartTime);
                                break;
                            case "endtime_asc":
                                bandwidthCallEventResponses = bandwidthCallEventResponses.OrderBy(x => x.EndTime);
                                break;
                            case "endtime_desc":
                                bandwidthCallEventResponses = bandwidthCallEventResponses.OrderByDescending(x => x.EndTime);
                                break;
                            case "callingnumber_asc":
                                bandwidthCallEventResponses = bandwidthCallEventResponses.OrderBy(x => x.CallingNumber);
                                break;
                            case "callingnumber_desc":
                                bandwidthCallEventResponses = bandwidthCallEventResponses.OrderByDescending(x => x.CallingNumber);
                                break;
                            case "callednumber_asc":
                                bandwidthCallEventResponses = bandwidthCallEventResponses.OrderBy(x => x.CalledNumber);
                                break;
                            case "callednumber_desc":
                                bandwidthCallEventResponses = bandwidthCallEventResponses.OrderByDescending(x => x.CalledNumber);
                                break;
                            case "calltype_asc":
                                bandwidthCallEventResponses = bandwidthCallEventResponses.OrderBy(x => x.CallType);
                                break;
                            case "calltype_desc":
                                bandwidthCallEventResponses = bandwidthCallEventResponses.OrderByDescending(x => x.CallType);
                                break;
                            case "calldirection_asc":
                                bandwidthCallEventResponses = bandwidthCallEventResponses.OrderBy(x => x.CallDirection);
                                break;
                            case "calldirection_desc":
                                bandwidthCallEventResponses = bandwidthCallEventResponses.OrderByDescending(x => x.CallDirection);
                                break;
                            case "duration_asc":
                                bandwidthCallEventResponses = bandwidthCallEventResponses.OrderBy(x => x.Duration);
                                break;
                            case "duration_desc":
                                bandwidthCallEventResponses = bandwidthCallEventResponses.OrderByDescending(x => x.Duration);
                                break;
                            case "callresult_asc":
                                bandwidthCallEventResponses = bandwidthCallEventResponses.OrderBy(x => x.CallResult);
                                break;
                            case "callresult_desc":
                                bandwidthCallEventResponses = bandwidthCallEventResponses.OrderByDescending(x => x.CallResult);
                                break;
                        }
                    }

                }

                return bandwidthCallEventResponses;
            }
            catch (Exception ex)
            {
                bandwidthCallCount = 0;
                Log.Error(ex, "An Error Occured in CallDetails Controller / Viewing All CallDetails :{ErrorMsg}", ex.Message);
            }
            return bandwidthCallEventResponses;

        }

        public async Task<BandwidthCallEventResponse> CallDetailsById(string id)
        {
            BandwidthCallEventResponse bandwidthCallEventResponses = null;

            try
            {
                bandwidthCallEventResponses = (from bce in Context.BandwidthCallEvents
                                               join os in Context.OrganizationServices on bce.SubAccount equals os.SubAccountId
                                               join o in Context.Organizations on os.OrganizationId equals o.OrganizationID
                                               join bm in Context.BandwidthMaster on bce.AccountId equals bm.AccountId

                                               where bce.CallId == id
                                               select new BandwidthCallEventResponse
                                               {
                                                   AccountId = bm.AccountName,
                                                   CallId = bce.CallId,
                                                   StartTime = bce.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                                   EndTime = bce.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                                   Duration = (bce.Duration.HasValue && bce.Duration != 0) ? FormatDuration(Convert.ToInt32(bce.Duration)) : null,
                                                   CallingNumber = bce.CallingNumber,
                                                   CalledNumber = bce.CalledNumber,
                                                   CallDirection = char.ToUpper(bce.CallDirection[0]) + bce.CallDirection.Substring(1).ToLower(),
                                                   CallType = char.ToUpper(bce.CallType[0]) + bce.CallType.Substring(1).ToLower(),
                                                   CallResult = char.ToUpper(bce.CallResult[0]) + bce.CallResult.Substring(1).ToLower(),
                                                   SipResponseCode = bce.SipResponseCode,
                                                   SipResponseDescription = bce.SipResponseDescription,
                                                   Cost = bce.Cost.ToString(),
                                                   SubAccountId = o.OrganizationName,
                                                   AttestationIndication = bce.AttestationIndicator,
                                                   HangUpSource = bce.HangUpSource,
                                                   PostDialDelay = bce.PostDialDelay,
                                                   PocketsSent = bce.PacketsSent,
                                                   PocketsReceived = bce.PacketsReceived,
                                                   CreateDate = bce.CreateDate,
                                                   IsActive = bce.IsActive
                                               }).FirstOrDefault();
                return bandwidthCallEventResponses;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in CallDetails Controller / Viewing All CallDetails :{ErrorMsg}", ex.Message);
            }
            return bandwidthCallEventResponses;

        }
        private static string FormatDuration(int milliseconds)
        {
            // Step 1: Convert milliseconds to seconds
            long totalSeconds = milliseconds / 1000;

            // Step 2: Calculate hours, minutes, and remaining seconds
            int hours = (int)(totalSeconds / 3600);
            int minutes = (int)((totalSeconds % 3600) / 60);
            int seconds = (int)(totalSeconds % 60);

            // Format the result as "hh:mm:ss"
            return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }

        public IEnumerable<CostAnalysisViewModel> GetAllCallsCostAnalysis(int skip, int take, string? searchValue, string sortOrder, string callDirection, string callType, string callResult, string orgName, out int callCount)
        {
            try
            {
                var lowercaseSearchValue = searchValue?.ToLower();
                var allCallEvents = (from bce in Context.BandwidthCallEvents
                                     join os in Context.OrganizationServices on bce.SubAccount equals os.SubAccountId
                                     join o in Context.Organizations on os.OrganizationId equals o.OrganizationID
                                     join bm in Context.BandwidthMaster on bce.AccountId equals bm.AccountId
                                     where os.ServiceType == Convert.ToInt32(ServiceType.Voice) && bce.IsActive
                                     orderby bce.Id
                                     select new CostAnalysisViewModel
                                     {
                                         StartTime = bce.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                         EndTime = bce.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                         Duration = (bce.Duration.HasValue && bce.Duration != 0) ? FormatDuration(Convert.ToInt32(bce.Duration)) : null,
                                         CallingNumber = bce.CallingNumber,
                                         CalledNumber = bce.CalledNumber,
                                         CallDirection = char.ToUpper(bce.CallDirection[0]) + bce.CallDirection.Substring(1).ToLower(),
                                         CallType = char.ToUpper(bce.CallType[0]) + bce.CallType.Substring(1).ToLower(),
                                         Cost = bce.Cost.ToString(),
                                         CallResult = char.ToUpper(bce.CallResult[0]) + bce.CallResult.Substring(1).ToLower(),
                                         OrganizationName = o.OrganizationName
                                     }).AsEnumerable();

                if (!string.IsNullOrEmpty(callDirection))
                {
                    allCallEvents = allCallEvents.Where(ce => ce.CallDirection.ToLower() == callDirection.ToLower()).AsEnumerable();
                }

                if (!string.IsNullOrEmpty(callType))
                {
                    allCallEvents = allCallEvents.Where(ce => ce.CallType.ToLower() == callType.ToLower()).AsEnumerable();
                }

                if (!string.IsNullOrEmpty(callResult))
                {
                    allCallEvents = allCallEvents.Where(ce => ce.CallResult.ToLower() == callResult.ToLower()).AsEnumerable();
                }

                if (!string.IsNullOrEmpty(orgName))
                {
                    allCallEvents = allCallEvents.Where(ce => ce.OrganizationName.ToLower() == orgName.ToLower()).AsEnumerable();
                }

                if (!string.IsNullOrEmpty(lowercaseSearchValue))
                {
                    allCallEvents = ApplySearch(allCallEvents, lowercaseSearchValue);
                }

                callCount = allCallEvents.Count();

                allCallEvents = allCallEvents.Skip(skip).Take(take);

                //sort data
                if (!string.IsNullOrEmpty(sortOrder))
                {
                    switch (sortOrder)
                    {
                        case "organizationname_asc":
                            allCallEvents = allCallEvents.OrderBy(x => x.OrganizationName);
                            break;
                        case "organizationname_desc":
                            allCallEvents = allCallEvents.OrderByDescending(x => x.OrganizationName);
                            break;
                        case "starttime_asc":
                            allCallEvents = allCallEvents.OrderBy(x => x.StartTime);
                            break;
                        case "starttime_desc":
                            allCallEvents = allCallEvents.OrderByDescending(x => x.StartTime);
                            break;
                        case "endtime_asc":
                            allCallEvents = allCallEvents.OrderBy(x => x.EndTime);
                            break;
                        case "endtime_desc":
                            allCallEvents = allCallEvents.OrderByDescending(x => x.EndTime);
                            break;
                        case "duration_asc":
                            allCallEvents = allCallEvents.OrderBy(x => x.Duration);
                            break;
                        case "duration_desc":
                            allCallEvents = allCallEvents.OrderByDescending(x => x.Duration);
                            break;
                        case "callingnumber_asc":
                            allCallEvents = allCallEvents.OrderBy(x => x.CallingNumber);
                            break;
                        case "callingnumber_desc":
                            allCallEvents = allCallEvents.OrderByDescending(x => x.CallingNumber);
                            break;
                        case "callednumber_asc":
                            allCallEvents = allCallEvents.OrderBy(x => x.CalledNumber);
                            break;
                        case "callednumber_desc":
                            allCallEvents = allCallEvents.OrderByDescending(x => x.CalledNumber);
                            break;
                        case "calldirection_asc":
                            allCallEvents = allCallEvents.OrderBy(x => x.CallDirection);
                            break;
                        case "calldirection_desc":
                            allCallEvents = allCallEvents.OrderByDescending(x => x.CallDirection);
                            break;
                        case "calltype_asc":
                            allCallEvents = allCallEvents.OrderBy(x => x.CallType);
                            break;
                        case "calltype_desc":
                            allCallEvents = allCallEvents.OrderByDescending(x => x.CallType);
                            break;
                        case "cost_asc":
                            allCallEvents = allCallEvents.OrderBy(x => x.Cost);
                            break;
                        case "cost_desc":
                            allCallEvents = allCallEvents.OrderByDescending(x => x.Cost);
                            break;
                        case "callresult_asc":
                            allCallEvents = allCallEvents.OrderBy(x => x.CallResult);
                            break;
                        case "callresult_desc":
                            allCallEvents = allCallEvents.OrderByDescending(x => x.CallResult);
                            break;
                    }
                }
                return allCallEvents;
            }
            catch (Exception ex)
            {
                callCount = 0;
                Log.Error(ex, "An Error Occured in Bandwidth Call Repository / Getting All Calls For Cost Analysis :{ErrorMsg}", ex.Message);
                return Enumerable.Empty<CostAnalysisViewModel>();
            }
        }

        private IEnumerable<CostAnalysisViewModel> ApplySearch(IEnumerable<CostAnalysisViewModel> allCallEvents, string lowercaseSearchValue)
        {
            allCallEvents = allCallEvents.Where(bce =>
                                         bce.OrganizationName.ToLower().Contains(lowercaseSearchValue) ||
                                         bce.StartTime.ToString().Contains(lowercaseSearchValue) ||
                                         bce.EndTime.ToString().Contains(lowercaseSearchValue) ||
                                         (bce.Duration != null && bce.Duration.Contains(lowercaseSearchValue)) ||
                                         bce.CallingNumber.ToString().Contains(lowercaseSearchValue) ||
                                         bce.CalledNumber.ToString().Contains(lowercaseSearchValue) ||
                                         bce.CallDirection.ToLower().Contains(lowercaseSearchValue) ||
                                         bce.CallType.ToLower().Contains(lowercaseSearchValue) ||
                                         (bce.Cost != null && bce.Cost.Contains(lowercaseSearchValue)) ||
                                         bce.CallResult.ToLower().Contains(lowercaseSearchValue));
            return allCallEvents;
        }

        public bool GetLoggedInUserInfo()
        {
            try
            {
                // Get the logged-in user
                var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                var currentUser = _userManager.Users.Where(u => u.Id == userId).FirstOrDefault();
                if (currentUser != null && currentUser.UserType == Convert.ToInt32(UserTypes.Thinkanew))
                {

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Bandwidth Call List Repository Controller / Geting LoggedIn User Information :{ErrorMsg}", ex.Message);
                return false;
            }
        }

        public string? GetOrganizationUserDetails()
        {
            try
            {
                // Get the logged-in user
                var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId != null)
                {
                    var userOrganizationDetails = Context.UserOrganizationFacilities.Where(o => o.UserId == userId).FirstOrDefault();
                    if (userOrganizationDetails != null)
                    {
                        return userOrganizationDetails.OrganizationID.ToString();
                    }
                    return null;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Bandwidth Call List Repository / Checking For User Is In SuperAdmin Role :{ErrorMsg}", ex.Message);
                return null;
            }
        }

        public DataTable GetCostSummaryByOrganization(CostSummaryCsvRequest costSummaryCsvRequest)
        {
            DataTable dataTable = new DataTable();
            try
            {
                var allCallEvents = (from bce in Context.BandwidthCallEvents
                                     join os in Context.OrganizationServices on bce.SubAccount equals os.SubAccountId
                                     join o in Context.Organizations on os.OrganizationId equals o.OrganizationID
                                     join bm in Context.BandwidthMaster on bce.AccountId equals bm.AccountId
                                     where os.ServiceType == Convert.ToInt32(ServiceType.Voice) && bce.IsActive
                                     orderby bce.Id
                                     select new CostAnalysisViewModel
                                     {
                                         StartTime = bce.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                         EndTime = bce.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                         Duration = (bce.Duration.HasValue && bce.Duration != 0) ? FormatDuration(Convert.ToInt32(bce.Duration)) : "NA",
                                         CallingNumber = bce.CallingNumber,
                                         CalledNumber = bce.CalledNumber,
                                         CallDirection = char.ToUpper(bce.CallDirection[0]) + bce.CallDirection.Substring(1).ToLower(),
                                         CallType = char.ToUpper(bce.CallType[0]) + bce.CallType.Substring(1).ToLower(),
                                         Cost = bce.Cost != null ? bce.Cost.ToString() : "NA",
                                         CallResult = char.ToUpper(bce.CallResult[0]) + bce.CallResult.Substring(1).ToLower(),
                                         OrganizationName = o.OrganizationName
                                     }).AsEnumerable();

                if (!string.IsNullOrEmpty(costSummaryCsvRequest.OrganizationName))
                {
                    allCallEvents = allCallEvents.Where(ce => ce.OrganizationName.ToLower() == costSummaryCsvRequest.OrganizationName.ToLower()).AsEnumerable();
                }
                if (!string.IsNullOrEmpty(costSummaryCsvRequest.CallDirection))
                {
                    allCallEvents = allCallEvents.Where(ce => ce.CallDirection.ToLower() == costSummaryCsvRequest.CallDirection.ToLower()).AsEnumerable();
                }

                if (!string.IsNullOrEmpty(costSummaryCsvRequest.CallType))
                {
                    allCallEvents = allCallEvents.Where(ce => ce.CallType.ToLower() == costSummaryCsvRequest.CallType.ToLower()).AsEnumerable();
                }

                if (!string.IsNullOrEmpty(costSummaryCsvRequest.CallResult))
                {
                    allCallEvents = allCallEvents.Where(ce => ce.CallResult.ToLower() == costSummaryCsvRequest.CallResult.ToLower()).AsEnumerable();
                }

                if(!string.IsNullOrEmpty(costSummaryCsvRequest.SearchedValue))
                {
                    allCallEvents = ApplySearch(allCallEvents, costSummaryCsvRequest.SearchedValue.ToLower());
                }

                dataTable.Columns.Clear();
                dataTable.Columns.Add(new DataColumn("OrganizationName"));
                dataTable.Columns.Add(new DataColumn("StartTime"));
                dataTable.Columns.Add(new DataColumn("EndTime"));
                dataTable.Columns.Add(new DataColumn("Duration"));
                dataTable.Columns.Add(new DataColumn("CallingNumber"));
                dataTable.Columns.Add(new DataColumn("CalledNumber"));
                dataTable.Columns.Add(new DataColumn("CallDirection"));
                dataTable.Columns.Add(new DataColumn("CallType"));
                dataTable.Columns.Add(new DataColumn("Cost"));
                dataTable.Columns.Add(new DataColumn("CallResult"));
                foreach (var item in allCallEvents)
                {
                    var dataRow = dataTable.NewRow();
                    dataRow["OrganizationName"] = item.OrganizationName;
                    dataRow["StartTime"] = item.StartTime;
                    dataRow["EndTime"] = item.EndTime;
                    dataRow["Duration"] = item.Duration;
                    dataRow["CallingNumber"] = item.CallingNumber;
                    dataRow["CalledNumber"] = item.CalledNumber;
                    dataRow["CallDirection"] = item.CallDirection;
                    dataRow["CallType"] = item.CallType;
                    dataRow["Cost"] = item.Cost;
                    dataRow["CallResult"] = item.CallResult;
                    dataTable.Rows.Add(dataRow);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetAllCallsCostAnalysis :{ErrorMsg}", ex.Message);
            }
            return dataTable;
        }
        public DataTable CallDetailsExportList(ExportCallHistoryRequest request)
        {
            DataTable result = new DataTable();
            try
            {
                var lowercaseSearchValue = request.SearchValue?.ToLower();
                var allCallEvents = (from bce in Context.BandwidthCallEvents
                                     join os in Context.OrganizationServices on bce.SubAccount equals os.SubAccountId
                                     join o in Context.Organizations on os.OrganizationId equals o.OrganizationID
                                     join bm in Context.BandwidthMaster on bce.AccountId equals bm.AccountId
                                     where os.ServiceType == Convert.ToInt32(ServiceType.Voice) &&
                                     (string.IsNullOrEmpty(lowercaseSearchValue) ||
                                     bm.AccountName.ToLower().Contains(lowercaseSearchValue) || o.OrganizationName.ToLower().Contains(lowercaseSearchValue) || bce.StartTime.ToString().Contains(lowercaseSearchValue)
                                     || bce.EndTime.ToString().Contains(lowercaseSearchValue) || bce.CallingNumber.ToLower().Contains(lowercaseSearchValue)
                                     || bce.CalledNumber.ToLower().Contains(lowercaseSearchValue) || bce.CallDirection.ToLower().Contains(lowercaseSearchValue)
                                     || bce.CallType.ToLower().Contains(lowercaseSearchValue) || bce.CallResult.ToLower().Contains(lowercaseSearchValue) || bce.Duration.ToString().Contains(lowercaseSearchValue))
                                     orderby bce.Id
                                     select new BandwidthCallEventResponse
                                     {
                                         OrganizationName = o.OrganizationName,
                                         CallId = bce.CallId,
                                         SubAccountId = bce.SubAccount,
                                         AccountId = bm.AccountName,
                                         StartTime = bce.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                         EndTime = bce.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                         Duration = (bce.Duration.HasValue && bce.Duration != 0) ? FormatDuration(Convert.ToInt32(bce.Duration)) : null,
                                         CallingNumber = bce.CallingNumber,
                                         CalledNumber = bce.CalledNumber,
                                         HangUpSource=bce.HangUpSource,
                                         CallDirection= char.ToUpper(bce.CallDirection[0]) + bce.CallDirection.Substring(1).ToLower(),
                                         CallType = char.ToUpper(bce.CallType[0]) + bce.CallType.Substring(1).ToLower(),
                                         CallResult = (char.ToUpper(bce.CallResult[0]) + bce.CallResult.Substring(1).ToLower()).ToString(),
                                     }).ToList();

                if (!string.IsNullOrEmpty(request.CallDirection))
                {
                    allCallEvents = allCallEvents.Where(ce => ce.CallDirection.ToLower() == request.CallDirection.ToLower()).ToList();
                }

                if (!string.IsNullOrEmpty(request.CallType))
                {
                    allCallEvents = allCallEvents.Where(ce => ce.CallType.ToLower() == request.CallType.ToLower()).ToList();
                }

                if (!string.IsNullOrEmpty(request.CallResult))
                {
                    allCallEvents = allCallEvents.Where(ce => ce.CallResult.ToLower() == request.CallResult.ToLower()).ToList();
                }

                if (!string.IsNullOrEmpty(request.OrganizationName))
                {
                    allCallEvents = allCallEvents.Where(ce => ce.OrganizationName.ToLower() == request.OrganizationName.ToLower()).ToList();
                }
                if (!string.IsNullOrEmpty(request.HangUpSource))
                {
                    allCallEvents = allCallEvents.Where(ce => ce.HangUpSource.ToLower() == request.HangUpSource.ToLower()).ToList();
                }

                result.Columns.Clear();
                result.Columns.Add(new DataColumn("SubAccountName"));
                result.Columns.Add(new DataColumn("StartTIme"));
                result.Columns.Add(new DataColumn("EndTime"));
                result.Columns.Add(new DataColumn("Duration"));
                result.Columns.Add(new DataColumn("CallingNumber"));
                result.Columns.Add(new DataColumn("CalledNumber"));
                result.Columns.Add(new DataColumn("CallDirection"));
                result.Columns.Add(new DataColumn("CallType"));
                result.Columns.Add(new DataColumn("CallResult"));
                foreach (var item in allCallEvents)
                {
                    var dataRow = result.NewRow();
                    dataRow["SubAccountName"] = item.OrganizationName;
                    dataRow["StartTIme"] = item.StartTime;
                    dataRow["EndTime"] = item.EndTime;
                    dataRow["Duration"] = item.Duration;
                    dataRow["CallingNumber"] = item.CallingNumber;
                    dataRow["CalledNumber"] = item.CalledNumber;
                    dataRow["CallDirection"] = item.CallDirection;
                    dataRow["CallType"] = item.CallType;
                    dataRow["CallResult"] = item.CallResult;
                    result.Rows.Add(dataRow);

                }

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Bandwidth Call details Export Repository  / Getting All Calls details For  :{ErrorMsg}", ex.Message);
            }
            return result;
        }

    }
}

