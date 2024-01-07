using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using TAN.ApplicationCore;
using TAN.DomainModels.Entities;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;

namespace TAN.Repository.Implementations
{
    [ExcludeFromCodeCoverage]
    public class HistoryRepository : Repository<History>, IHistory
    {
        private readonly IHttpContextAccessor _httpContext;
        private DatabaseContext Context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public HistoryRepository(DbContext db, IHttpContextAccessor httpContext)
        {
            this.db = db;
            _httpContext = httpContext;
        }
        public async Task<bool> AddHistory(History history)
        {
            try
            {
                if (history != null)
                {
                    var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (history.Id == 0)
                    {
                        history.CreateDate = DateTime.UtcNow;
                        history.CreateBy = userId?.ToString();
                        await Context.History.AddAsync(history);
                        Context.SaveChanges();
                        return true;
                    }
                    else
                    {
                        history.UpdateDate = DateTime.UtcNow;
                        history.UpdateBy = userId?.ToString();
                        Context.History.Update(history);
                        Context.SaveChanges();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured AddHistory :{ErrorMsg}", ex.Message);
            }
            return false;
        }

        public async Task<bool> UploadHistory(UploadHistory uploadHistory)
        {
            try
            {
                if (uploadHistory != null)
                {
                    var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (uploadHistory.Id == 0)
                    {
                        uploadHistory.CreatedDate = DateTime.UtcNow;
                        uploadHistory.CreatedBy = userId;
                        await Context.UploadHistory.AddAsync(uploadHistory);
                        Context.SaveChanges();
                        return true;
                    }
                    else
                    {
                        uploadHistory.UpdatedDate = DateTime.UtcNow;
                        uploadHistory.UpdatedBy = userId;
                        Context.UploadHistory.Update(uploadHistory);
                        Context.SaveChanges();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in History Controller / Uploading Submission History :{ErrorMsg}", ex.Message);
            }
            return false;
        }

        public IEnumerable<SubmissionHistoryViewModel> GetAllSubmissionHistory(int skip, int take, string? searchValue, string sortOrder, string orgName, string facilityName, out int submissionCount)
        {
            try
            {
                var lowercaseSearchValue = searchValue?.ToLower();
                var status = string.Empty;
                if (lowercaseSearchValue == "pending")
                {
                    status = "0";
                }
                else if (lowercaseSearchValue == "accepted")
                {
                    status = "1";
                }
                else if (lowercaseSearchValue == "rejected")
                {
                    status = "2";
                }
                var submissions = (from h in Context.History
                                   join o in Context.Organizations on h.OrganizationId equals o.OrganizationID
                                   join f in Context.Facilities on h.FacilityId equals f.Id
                                   where h.IsActive && f.OrganizationId == o.OrganizationID
                                   orderby h.Id
                                   select new SubmissionHistoryViewModel
                                   {
                                       Id = h.Id,
                                       FileName = h.FileName,
                                       FacilityId = h.FacilityId,
                                       Status = h.Status,
                                       IsActive = h.IsActive,
                                       SubmissionDate = h.CreateDate.Date.ToString("yyyy-MM-dd"),
                                       OrganizationId = h.OrganizationId,
                                       FacilityName = f.FacilityName,
                                       OrganizationName = o.OrganizationName
                                   }).AsEnumerable();

                if (!string.IsNullOrEmpty(lowercaseSearchValue))
                {
                    submissions = ApplySearch(submissions, lowercaseSearchValue, status);
                }

                if (orgName != null)
                {
                    var organizationDetail = Context.Organizations.Where(o => o.OrganizationName == orgName).FirstOrDefault();
                    if (organizationDetail != null)
                    {
                        submissions = submissions.Where(s => s.OrganizationId == organizationDetail.OrganizationID).AsEnumerable();
                    }
                }

                if (facilityName != null)
                {
                    var facilityDetail = Context.Facilities.Where(f => f.FacilityName == facilityName).FirstOrDefault();
                    if (facilityDetail != null)
                    {
                        submissions = submissions.Where(s => s.FacilityId == facilityDetail.Id).AsEnumerable();
                    }
                }

                submissionCount = submissions.Count();

                submissions = submissions.Skip(skip).Take(take);


                //sort data
                if (!string.IsNullOrEmpty(sortOrder))
                {
                    switch (sortOrder)
                    {
                        case "submissiondate_asc":
                            submissions = submissions.OrderBy(x => x.SubmissionDate);
                            break;
                        case "submissiondate_desc":
                            submissions = submissions.OrderByDescending(x => x.SubmissionDate);
                            break;
                        case "filename_asc":
                            submissions = submissions.OrderBy(x => x.FileName);
                            break;
                        case "filename_desc":
                            submissions = submissions.OrderByDescending(x => x.FileName);
                            break;
                        case "status_asc":
                            submissions = submissions.OrderBy(x => x.Status);
                            break;
                        case "status_desc":
                            submissions = submissions.OrderByDescending(x => x.Status);
                            break;
                        case "facilityname_asc":
                            submissions = submissions.OrderBy(x => x.FacilityName);
                            break;
                        case "facilityname_desc":
                            submissions = submissions.OrderByDescending(x => x.FacilityName);
                            break;
                        case "organizationname_asc":
                            submissions = submissions.OrderBy(x => x.OrganizationName);
                            break;
                        case "organizationname_desc":
                            submissions = submissions.OrderByDescending(x => x.OrganizationName);
                            break;

                    }
                }
                return submissions;
            }
            catch (Exception ex)
            {
                submissionCount = 0;
                Log.Error(ex, "An Error Occured in History Controller / Viewing All Submissions :{ErrorMsg}", ex.Message);
                return Enumerable.Empty<SubmissionHistoryViewModel>();
            }
        }

        private IEnumerable<SubmissionHistoryViewModel> ApplySearch(IEnumerable<SubmissionHistoryViewModel> query, string lowercaseSearchValue, string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                query = query.Where(sub =>
                sub.SubmissionDate.ToString().Contains(lowercaseSearchValue) ||
                sub.FacilityName.ToLower().Contains(lowercaseSearchValue) ||
                sub.OrganizationName.ToLower().Contains(lowercaseSearchValue) ||
                sub.FileName.ToLower().Contains(lowercaseSearchValue));
            }
            else
            {
                query = query.Where(sub =>
                sub.Status == Convert.ToInt32(status));
            }
            return query;
        }

        public async Task<string> DeleteSubmissionHistoryDetail(string fileName)
        {
            try
            {
                var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                var fileDetails = Context.History.Where(h => h.FileName == fileName).FirstOrDefault();
                var fileNameWithoutExtension = Path.ChangeExtension(fileName, null);
                var uploadfileDetails = Context.UploadHistory.Where(up => up.FileName == fileNameWithoutExtension).FirstOrDefault();
                if (fileDetails != null)
                {
                    fileDetails.IsActive = false;
                    fileDetails.UpdateBy = userId;
                    fileDetails.UpdateDate = DateTime.UtcNow;
                    Context.History.Update(fileDetails);
                    await Context.SaveChangesAsync();

                    if (uploadfileDetails != null)
                    {
                        uploadfileDetails.IsActive = false;
                        uploadfileDetails.UpdatedBy = userId;
                        uploadfileDetails.UpdatedDate = DateTime.UtcNow;
                        Context.UploadHistory.Update(uploadfileDetails);
                        await Context.SaveChangesAsync();
                    }
                    return "success";
                }
                else
                {
                    return "failed";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in History Controller / Delete History of Submitted file  :{ErrorMsg}", ex.Message);
                return "failed";
            }
        }

        public async Task<SubmissionHistoryViewModel> GetDetails(string fileName)
        {
            try
            {
                var fileData = await Context.History.Where(h => h.FileName == fileName).FirstOrDefaultAsync();
                SubmissionHistoryViewModel submissionHistoryViewModel = new SubmissionHistoryViewModel()
                {
                    Id = fileData.Id,
                    FileName = fileData.FileName,
                    CreateDate = fileData.CreateDate,
                    Status = fileData.Status
                };

                return submissionHistoryViewModel;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in History Controller / Getting Submission File Details :{ErrorMsg}", ex.Message);
                return new SubmissionHistoryViewModel();
            }
        }

        public async Task<string> UpdateSubmissionStatus(string fileId, string? fileDate, string? fileName, string? fileStatus)
        {
            try
            {
                var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                var fileDetails = await Context.History.Where(h => h.Id == Convert.ToInt32(fileId)).FirstOrDefaultAsync();
                if (fileDetails != null)
                {
                    fileDetails.Status = Convert.ToInt32(fileStatus);
                    fileDetails.UpdateDate = DateTime.UtcNow;
                    fileDetails.UpdateBy = userId;

                    Context.History.Update(fileDetails);
                    await Context.SaveChangesAsync();
                    return "success";
                }
                else
                {
                    return "failed";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Histort Controller / Updating Submission Data :{ErrorMsg}", ex.Message);
                return "failed";
            }
        }

        public async Task<int> GetLastQuarterSubmitHistory(int Quarter, int OrgId)
        {
            int percentage = 0;

            if (OrgId > 0)
            {
                var submissions = (from h in Context.History
                                   join f in Context.Facilities on h.FacilityId equals f.Id
                                   join o in Context.Organizations on h.OrganizationId equals o.OrganizationID
                                   where h.IsActive && h.ReportQuarter == Quarter && h.Year == DateTime.Now.Year && f.IsActive && o.IsActive && o.OrganizationID == OrgId
                                   select h).ToList();
                var SubmissionCount = submissions.GroupBy(c => c.FacilityId).Select(g => new Itemlist { Text = g.Key.ToString(), Value = g.Count() }).Count();
                var facility = from f in Context.Facilities.Where(f => f.IsActive && f.OrganizationId == OrgId) select f.FacilityID;
                var facilityCount = facility.Count();
                var ratio = (double)SubmissionCount / facilityCount;
                percentage = Convert.ToInt32(ratio * 100);
                return percentage;
            }
            else
            {
                var submissions = (from h in Context.History
                                   join f in Context.Facilities on h.FacilityId equals f.Id
                                   join o in Context.Organizations on h.OrganizationId equals o.OrganizationID
                                   where h.IsActive && h.ReportQuarter == Quarter && h.Year == DateTime.Now.Year && f.IsActive && o.IsActive
                                   select h).ToList();
                var SubmissionCount = submissions.GroupBy(c => c.FacilityId).Select(g => new Itemlist { Text = g.Key.ToString(), Value = g.Count() }).Count();
                var facility = from f in Context.Facilities.Where(f => f.IsActive) select f.FacilityID;
                var facilityCount = facility.Count();
                var ratio = (double)SubmissionCount / facilityCount;
                percentage = Convert.ToInt32(ratio * 100);
                return percentage;
            }
        }

        public async Task<List<FacilityModel>> GetPendingSubmit(int Quarter, int OrgId)
        {
            if (OrgId > 0)
            {
                var submissions = (from h in Context.History
                                   join f in Context.Facilities on h.FacilityId equals f.Id
                                   join o in Context.Organizations on h.OrganizationId equals o.OrganizationID
                                   where h.IsActive && h.Year == DateTime.Now.Year && h.ReportQuarter == Quarter && f.IsActive && o.IsActive && o.OrganizationID == OrgId
                                   select f).ToList();
                var Submitted = submissions.Select(x => x.Id).Distinct();
                var facilityIds = Context.Facilities.Where(a => a.IsActive && a.OrganizationId == OrgId).Select(f => f.Id);
                var pending = new List<int>();
                List<FacilityModel> facilities = new List<FacilityModel>();
                //Get Pending Facility id
                foreach (var id in facilityIds)
                {
                    if (!Submitted.Contains(id))
                    {
                        pending.Add(id);
                    }
                }
                //Get Pending Facilities
                foreach (var id in pending)
                {
                    var facility = Context.Facilities.Where(c => c.IsActive).FirstOrDefault(f => f.Id == id);
                    facilities.Add(facility);
                }

                return facilities;
            }
            else
            {
                var submissions = (from h in Context.History
                                   join f in Context.Facilities on h.FacilityId equals f.Id
                                   join o in Context.Organizations on h.OrganizationId equals o.OrganizationID
                                   where h.IsActive && h.Year == DateTime.Now.Year && h.ReportQuarter == Quarter && f.IsActive && o.IsActive
                                   select f).ToList();
                var Submitted = submissions.Select(x => x.Id).Distinct();
                var facilityIds = Context.Facilities.Where(a => a.IsActive).Select(f => f.Id);
                var pending = new List<int>();
                List<FacilityModel> facilities = new List<FacilityModel>();
                //Get Pending Facility id
                foreach (var id in facilityIds)
                {
                    if (!Submitted.Contains(id))
                    {
                        pending.Add(id);
                    }
                }
                //Get Pending Facilities
                foreach (var id in pending)
                {
                    var facility = Context.Facilities.Where(c => c.IsActive).FirstOrDefault(f => f.Id == id);
                    facilities.Add(facility);
                }

                return facilities;
            }

        }

        public async Task<List<Itemlist>> GetSubmitStatus(int Quarter, int OrgId)
        {
            var newstatusList = new List<Itemlist>();
            var facilityIds = (dynamic)null;
            if (OrgId > 0)
            {
                facilityIds = Context.Facilities.Where(c => c.OrganizationId == OrgId && c.IsActive).Select(f => f.Id).Count();
            }
            else
            {
                facilityIds = Context.Facilities.Where(c => c.IsActive).Select(f => f.Id).Count();
            }
            var statusList = new List<Itemlist>();
            if (OrgId != 0)
            {
                var maxCreateDates = (
               from h in Context.History
               where h.IsActive && h.ReportQuarter == Quarter
               group h by h.FacilityId into facilityGroup
               select new
               {
                   FacilityId = facilityGroup.Key,
                   MaxCreateDate = facilityGroup.Max(h => h.CreateDate)
               }).ToList();

                var facilities = Context.Facilities
                    .Where(f => f.IsActive)
                    .ToList();

                var submissions = (
                    from f in facilities
                    join maxDate in maxCreateDates on f.Id equals maxDate.FacilityId
                    join h in Context.History on new { FacilityId = f.Id, CreateDate = maxDate.MaxCreateDate } equals new { h.FacilityId, h.CreateDate }
                    join r in Context.References on h.Status equals r.RefereceId
                    where h.IsActive && h.ReportQuarter == Quarter && f.OrganizationId==OrgId
                    select new Itemlist
                    {
                        Text = r.Name,
                        Value = f.Id
                    }
                ).ToList();

                statusList = submissions.GroupBy(x => x.Text).Select(grp => new Itemlist { Text = grp.Key, Value = grp.Count() }).ToList();
                foreach (var item in statusList)
                {
                    var x = new Itemlist();

                    x.Text = item.Text;
                    var ratio = (double)item.Value / facilityIds;
                    var percent = ratio * 100;
                    x.Value = Convert.ToInt32(percent);

                    newstatusList.Add(x);

                }
                var requiredValues = new List<string> { "Submitted", "Pending", "Accepted", "Rejected" };

                foreach (var requiredValue in requiredValues)
                {
                    // Check if the requiredValue is not in newstatusList.Text
                    if (!newstatusList.Any(item => item.Text == requiredValue))
                    {
                        // Add a new item with the requiredValue and Value of 0
                        newstatusList.Add(new Itemlist { Text = requiredValue, Value = 0 });
                    }
                }

                // Sort the newstatusList alphabetically by Text
                newstatusList = newstatusList.OrderBy(item => item.Text).ToList();


            }
            else
            {
                var maxCreateDates = (
                from h in Context.History
                where h.IsActive && h.ReportQuarter == Quarter
                group h by h.FacilityId into facilityGroup
                select new
                {
                    FacilityId = facilityGroup.Key,
                    MaxCreateDate = facilityGroup.Max(h => h.CreateDate)
                }).ToList();

                var facilities = Context.Facilities
                    .Where(f => f.IsActive)
                    .ToList();

                var submissions = (
                    from f in facilities
                    join maxDate in maxCreateDates on f.Id equals maxDate.FacilityId
                    join h in Context.History on new { FacilityId = f.Id, CreateDate = maxDate.MaxCreateDate } equals new { h.FacilityId, h.CreateDate }
                    join r in Context.References on h.Status equals r.RefereceId
                    where h.IsActive && h.ReportQuarter == Quarter
                    select new Itemlist
                    {
                        Text = r.Name,
                        Value = f.Id
                    }
                ).ToList();

                statusList = submissions.GroupBy(x => x.Text).Select(grp => new Itemlist { Text = grp.Key, Value = grp.Count() }).ToList();
                foreach (var item in statusList)
                {
                    var x = new Itemlist();

                    x.Text = item.Text;
                    var ratio = (double)item.Value / facilityIds;
                    var percent = ratio * 100;
                    x.Value = Convert.ToInt32(percent);

                    newstatusList.Add(x);

                }
                var requiredValues = new List<string> { "Submitted", "Pending", "Accepted", "Rejected" };

                foreach (var requiredValue in requiredValues)
                {
                    // Check if the requiredValue is not in newstatusList.Text
                    if (!newstatusList.Any(item => item.Text == requiredValue))
                    {
                        // Add a new item with the requiredValue and Value of 0
                        newstatusList.Add(new Itemlist { Text = requiredValue, Value = 0 });
                    }
                }

                // Sort the newstatusList alphabetically by Text
                newstatusList = newstatusList.OrderBy(item => item.Text).ToList();

            }

            return newstatusList;
        }

    }
}
