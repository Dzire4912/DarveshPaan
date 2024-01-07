using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Models;
using TAN.DomainModels.ViewModels;

namespace TAN.Repository.Abstractions
{
    public interface ICalls : IRepository<BandwidthCallEvents>
    {
        Task<bool> AddCalls(List<BandwidthCallEvents> calls);
        Task<List<string>> CheckDuplicateCallsId(List<string> callIds);
        IEnumerable<BandwidthCallEventResponse> CallDetails(string? searchValue, string? sortOrder, string skip, string take, out int bandwidthCallCount, BandWidthCallFilterViewModel bandWidthCallFilterViewModel);
        Task<BandwidthCallEventResponse> CallDetailsById(string id);
        IEnumerable<CostAnalysisViewModel> GetAllCallsCostAnalysis(int skip, int take, string? searchValue, string sortOrder, string callDirection, string callType, string callResult, string orgName, out int callCount);
        bool GetLoggedInUserInfo();
        string? GetOrganizationUserDetails();
        DataTable GetCostSummaryByOrganization(CostSummaryCsvRequest costSummaryCsvRequest);
        DataTable CallDetailsExportList(ExportCallHistoryRequest request);
    }
}
