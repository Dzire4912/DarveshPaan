using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using TAN.DomainModels.ViewModels;

namespace TAN.Repository.Abstractions
{
    public interface IHistory
    {
        Task<bool> AddHistory(History history);
        IEnumerable<SubmissionHistoryViewModel> GetAllSubmissionHistory(int skip, int take, string? searchValue, string sortOrder, string orgName, string facilityName, out int submissionCount);
        Task<string> DeleteSubmissionHistoryDetail(string fileName);
        Task<SubmissionHistoryViewModel> GetDetails(string fileName);
        Task<string> UpdateSubmissionStatus(string fileId, string? fileDate, string? fileName, string? fileStatus);
        Task<bool> UploadHistory(UploadHistory uploadHistory);
        Task<int> GetLastQuarterSubmitHistory(int Quarter ,int OrgId);
        Task<List<FacilityModel>> GetPendingSubmit(int Quarter, int OrgId);
        Task<List<Itemlist>> GetSubmitStatus(int Quarter, int OrgId);
    }
}
