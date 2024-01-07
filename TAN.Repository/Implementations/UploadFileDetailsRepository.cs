using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Helpers;
using TAN.ApplicationCore;
using TAN.ApplicationCore.Migrations;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.Models;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static TAN.DomainModels.Helpers.Permissions;
using static TAN.DomainModels.Models.UploadModel;
using static TAN.DomainModels.Models.XMLReaderModel;

namespace TAN.Repository.Implementations
{
    [ExcludeFromCodeCoverage]
    public class UploadFileDetailsRepository : Repository<UploadFileDetails>, IUploadFileDetails
    {
        private readonly IHttpContextAccessor _httpContext;
        private DatabaseContext Context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public UploadFileDetailsRepository(DbContext db, IHttpContextAccessor httpContext)
        {
            this.db = db;
            _httpContext = httpContext;
        }

        public List<UploadFileDetails> GetUploadFileDetails(int FacilityId, int PageSize, int PageNo, out int TotalCount)
        {
            List<UploadFileDetails> uploadFiles = new List<UploadFileDetails>();
            TotalCount = 0;
            try
            {
                uploadFiles = Context.UploadFileDetails.Where(x => x.IsActive && x.FacilityID == FacilityId).Select(s => new UploadFileDetails()
                {
                    Id = s.Id,
                    FileName = s.FileName,
                    RowCount = s.RowCount,
                    CreatedDate = s.CreatedDate
                }).ToList();
                if (uploadFiles.Count > 0)
                {
                    TotalCount = uploadFiles.Count;
                    uploadFiles = uploadFiles.Skip(PageNo).Take(PageSize).ToList();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetUploadFileDetails :{ErrorMsg}", ex.Message);
            }
            return uploadFiles;
        }
        public async Task<UploadFileDetails> GetUploadFileDetailsByFileName(string Filename, string facilityId)
        {
            UploadFileDetails uploadFiles = new UploadFileDetails();
            try
            {
                var _uploadFiles = await Context.UploadFileDetails.Where(x => x.IsActive
                && x.FileName == Filename
                && x.FacilityID == Convert.ToInt32(facilityId)).
                Select(z => z.FileName).FirstOrDefaultAsync();

                if (_uploadFiles != null)
                {
                    uploadFiles.FileName = _uploadFiles.ToString();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetUploadFileDetailsByFileName :{ErrorMsg}", ex.Message);
            }
            return uploadFiles;
        }

        public async Task<UploadFileDetails> AddFileDetails(string Filename, string facilityId, int RowCount, RequestModel requestModel)
        {
            UploadFileDetails uploadFiles = new UploadFileDetails();
            try
            {
                uploadFiles.CreatedDate = DateTime.Now;
                uploadFiles.FileName = Filename;
                uploadFiles.FacilityID = Convert.ToInt32(facilityId);
                uploadFiles.RowCount = RowCount;
                uploadFiles.IsActive = true;
                uploadFiles.ReportQuarter = requestModel.Quarter;
                uploadFiles.Year = requestModel.year;
                uploadFiles.Month = requestModel.Month;
                uploadFiles.CreatedBy = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                await Context.UploadFileDetails.AddAsync(uploadFiles);
                await Context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured AddFileDetails :{ErrorMsg}", ex.Message);
            }
            return uploadFiles;
        }

        public List<FileDetailsResponse> GetFileRecord(BackoutTableRequest request, int PageSize, int PageNo, out int TotalCount, string sortOrder)
        {
            List<FileDetailsResponse> fileDetails = new List<FileDetailsResponse>();
            TotalCount = 0;
            try
            {
                fileDetails = (from u in Context.UploadFileDetails
                               join f in Context.Facilities on u.FacilityID equals f.Id
                               where u.IsActive && f.IsActive && u.FacilityID == request.FacilityID
                               && u.Year == request.Year && u.ReportQuarter == request.ReportQuarter
                               && (string.IsNullOrEmpty(request.Search) || u.FileName.Contains(request.Search))
                               select new FileDetailsResponse
                               {
                                   FileDetailsId = u.Id,
                                   FileName = u.FileName,
                                   InvalidRecord = Context.FileErrorRecords.Count(er => er.IsActive && er.FileDetailId == u.Id && er.Status == 0),
                                   ValidRecord = Context.Timesheet.Count(ts => ts.IsActive && ts.FileDetailId == u.Id),
                                   RowCount = u.RowCount,
                                   CreateDate = u.CreatedDate.ToString("MM-dd-yyyy HH:mm:ss"),
                                   FacilityID = f.Id.ToString(),
                                   FacilityName = f.FacilityName.Replace("'", "\\'"),
                                   Year = u.Year,
                                   Month = u.Month,
                                   ReportQuarter = u.ReportQuarter
                               }).ToList();



                if (fileDetails.Count > 0)
                {
                    if (request.Status == (int)BackoutRequestStatus.Invalid)
                    {
                        fileDetails = fileDetails.Where(x => x.InvalidRecord > 0).ToList();
                    }
                    else if (request.Status == (int)BackoutRequestStatus.Valid)
                    {
                        fileDetails = fileDetails.Where(x => x.ValidRecord > 0).ToList();
                    }

                    TotalCount = fileDetails.Count;
                    fileDetails = fileDetails.Skip(PageNo).Take(PageSize).ToList();
                    if (!string.IsNullOrEmpty(sortOrder))
                    {
                        switch (sortOrder)
                        {
                            case "filename_asc":
                                fileDetails = fileDetails.OrderBy(x => x.FileName).ToList();
                                break;
                            case "filename_desc":
                                fileDetails = fileDetails.OrderByDescending(x => x.FileName).ToList();
                                break;
                            case "validrecord_asc":
                                fileDetails = fileDetails.OrderBy(x => x.ValidRecord).ToList();
                                break;
                            case "validrecord_desc":
                                fileDetails = fileDetails.OrderByDescending(x => x.ValidRecord).ToList();
                                break;
                            case "rowcount_asc":
                                fileDetails = fileDetails.OrderBy(x => x.RowCount).ToList();
                                break;
                            case "rowcount_desc":
                                fileDetails = fileDetails.OrderByDescending(x => x.RowCount).ToList();
                                break;
                            case "createdate_asc":
                                fileDetails = fileDetails.OrderBy(x => x.CreateDate).ToList();
                                break;
                            case "createdate_desc":
                                fileDetails = fileDetails.OrderByDescending(x => x.CreateDate).ToList();
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in AddFileDetails :{ErrorMsg}", ex.Message);
            }
            return fileDetails;
        }

        public async Task<BackOutAllRecordResponse> BackOutAllRecord(BackOutAllRecordRequest request)
        {
            BackOutAllRecordResponse response = new BackOutAllRecordResponse();
            response.status = false;
            try
            {
                var data = await Context.UploadFileDetails.Where(x => x.IsActive && x.Id == request.FileDetailsId).DefaultIfEmpty().FirstOrDefaultAsync();
                if (data != null)
                {
                    data.IsActive = false;
                    Context.UploadFileDetails.Update(data);
                    await Context.SaveChangesAsync();
                    Context.Timesheet.Where(x => x.IsActive && x.FacilityId == request.FacilityId && x.FileDetailId == request.FileDetailsId).ExecuteUpdate(e => e.SetProperty(s => s.IsActive, s => !s.IsActive));
                    await Context.SaveChangesAsync();
                    response.status = true;
                    response.Message = "Deleted All records";
                    return response;
                }
                response.Message = "File is missing";
            }
            catch (Exception ex)
            {
                response.Message = "Failed to Delete";
                Log.Error(ex, "An Error Occured in BackOutAllRecord :{ErrorMsg}", ex.Message);
            }
            return response;
        }

        public List<FileErrorList> GetFileErrorList(BackOutAllRecordRequest request, int PageSize, int PageNo, out int TotalCount, string sortOrder)
        {
            List<FileErrorList> fileErrorList = new List<FileErrorList>();
            List<FileErrorList> updatedFileErrorList = new List<FileErrorList>();
            TotalCount = 0;
            try
            {
                fileErrorList = (from fe in Context.FileErrorRecords
                                 join f in Context.Facilities on fe.FacilityId equals f.Id
                                 join u in Context.UploadFileDetails on fe.FileDetailId equals u.Id
                                 where fe.IsActive && f.IsActive && u.IsActive && fe.FacilityId == request.FacilityId && fe.FileDetailId == request.FileDetailsId && fe.Status == 0
                                 select new FileErrorList
                                 {
                                     Id = fe.Id,
                                     ErrorMessage = fe.ErrorMessage,
                                     FileRowData = fe.FileRowData,
                                     FacilityId = fe.FacilityId,
                                     FacilityName = f.FacilityName,
                                     Year = fe.Year,
                                     ReportQuarter = fe.ReportQuarter,
                                     Month = fe.Month,
                                     FileDetailId = fe.FileDetailId,
                                     FileName = u.FileName
                                 }).ToList();

                foreach (var fileError in fileErrorList)
                {
                    var jsonArray = JsonConvert.DeserializeObject<JArray>(fileError.FileRowData);

                    FileRowData fileRowData = new FileRowData();
                    foreach (var jsonObject in jsonArray)
                    {
                        var property = (JProperty)jsonObject.First;
                        var propertyName = property.Name;
                        var propertyValue = property.Value;

                        if (propertyName == "Employee Id") fileRowData.EmployeeId = propertyValue.ToString();
                        else if (propertyName == "Pay Type Code") fileRowData.PayTypeCode = Convert.ToInt32(propertyValue.ToString());
                        else if (propertyName == "Work Day" || propertyName == "Workday") fileRowData.WorkDay = propertyValue.ToString();
                        else if (propertyName == "Hours") fileRowData.Hours = Convert.ToInt32(propertyValue.ToString());
                        else if (propertyName == "Job Title Code") fileRowData.JobTitleCode = Convert.ToInt32(propertyValue.ToString());
                        else if (propertyName == "First Name" || propertyName == "Name") fileRowData.EmployeeFirstName = propertyValue.ToString();
                    }

                    FileErrorList tempFileErrorList = new FileErrorList();
                    tempFileErrorList.finalFileRowData = fileRowData;
                    tempFileErrorList.Id = fileError.Id;
                    tempFileErrorList.ErrorMessage = fileError.ErrorMessage;
                    tempFileErrorList.FileRowData = fileError.FileRowData;
                    tempFileErrorList.FacilityId = fileError.FacilityId;
                    tempFileErrorList.FacilityName = fileError.FacilityName;
                    tempFileErrorList.Year = fileError.Year;
                    tempFileErrorList.ReportQuarter = fileError.ReportQuarter;
                    tempFileErrorList.Month = fileError.Month;
                    tempFileErrorList.FileDetailId = fileError.FileDetailId;
                    tempFileErrorList.FileName = fileError.FileName;

                    updatedFileErrorList.Add(tempFileErrorList);

                }

                if (updatedFileErrorList.Count > 0)
                {
                    TotalCount = updatedFileErrorList.Count;
                    fileErrorList = updatedFileErrorList.Skip(PageNo).Take(PageSize).ToList();
                    if (!string.IsNullOrEmpty(sortOrder))
                    {
                        switch (sortOrder)
                        {
                            case "errormessage_asc":
                                fileErrorList = fileErrorList.OrderBy(x => x.ErrorMessage).ToList();
                                break;
                            case "errormessage_desc":
                                fileErrorList = fileErrorList.OrderByDescending(x => x.ErrorMessage).ToList();
                                break;
                            case "employee id_asc":
                                fileErrorList = fileErrorList.OrderBy(x => x.finalFileRowData.EmployeeId).ToList();
                                break;
                            case "employee id_desc":
                                fileErrorList = fileErrorList.OrderByDescending(x => x.finalFileRowData.EmployeeId).ToList();
                                break;
                            case "pay type code_asc":
                                fileErrorList = fileErrorList.OrderBy(x => x.finalFileRowData.PayTypeCode).ToList();
                                break;
                            case "pay type code_desc":
                                fileErrorList = fileErrorList.OrderByDescending(x => x.finalFileRowData.PayTypeCode).ToList();
                                break;
                            case "work day_asc":
                                fileErrorList = fileErrorList.OrderBy(x => x.finalFileRowData.WorkDay).ToList();
                                break;
                            case "work day_desc":
                                fileErrorList = fileErrorList.OrderByDescending(x => x.finalFileRowData.WorkDay).ToList();
                                break;
                            case "hours_asc":
                                fileErrorList = fileErrorList.OrderBy(x => x.finalFileRowData.Hours).ToList();
                                break;
                            case "hours_desc":
                                fileErrorList = fileErrorList.OrderByDescending(x => x.finalFileRowData.Hours).ToList();
                                break;
                            case "job title code_asc":
                                fileErrorList = fileErrorList.OrderBy(x => x.finalFileRowData.JobTitleCode).ToList();
                                break;
                            case "job title code_desc":
                                fileErrorList = fileErrorList.OrderByDescending(x => x.finalFileRowData.JobTitleCode).ToList();
                                break;
                            case "employee first name_asc":
                                fileErrorList = fileErrorList.OrderBy(x => x.finalFileRowData.EmployeeFirstName).ToList();
                                break;
                            case "employee first name_desc":
                                fileErrorList = fileErrorList.OrderByDescending(x => x.finalFileRowData.EmployeeFirstName).ToList();
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in GetFileErrorList :{ErrorMsg}", ex.Message);
            }
            return fileErrorList;
        }

        public async Task<ErrorData> GetErrorRawData(int Id)
        {
            ErrorData errorData = new ErrorData();
            try
            {
                var data = await Context.FileErrorRecords.FirstOrDefaultAsync(x => x.IsActive && x.Id == Id);
                if (data != null)
                {
                    errorData.keyValueList = ConvertJsonToList(data.FileRowData);
                    errorData.Id = data.Id;
                    errorData.FileDetailId = data.FileDetailId;
                    errorData.FacilityId = data.FacilityId;
                    errorData.ReportQuarter = data.ReportQuarter;
                    errorData.Year = data.Year;
                    errorData.Month = data.Month;
                    errorData.UploadType = data.UploadType;

                    string employeeId = Convert.ToString(errorData.keyValueList.FirstOrDefault(pair => pair.Key == "Employee Id").Value);
                    string workday = Convert.ToString(errorData.keyValueList.FirstOrDefault(pair => pair.Key == "Work Day").Value);
                    if (employeeId != null && employeeId != "" && workday != null && workday != "")
                    {
                        errorData.WorkdayList = await GetWorkDayListByEmployee(new EmployeeWorkDayRequest() { EmployeeId = employeeId, FacilityId = errorData.FacilityId, WorkDay = Convert.ToDateTime(workday) });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in GetErrorRawData :{ErrorMsg}", ex.Message);
            }
            return errorData;
        }

        public async Task<bool> DeleteErrorRawData(int Id)
        {
            try
            {
                var data = await Context.FileErrorRecords.Where(x => x.Id == Convert.ToInt32(Id)).FirstOrDefaultAsync();
                if (data != null)
                {
                    data.IsActive = false;
                    data.UpdateDDate = DateTime.Now;
                    data.UpdateDBy = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                    Context.Update(data);
                    await Context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in DeleteErrorRawData :{ErrorMsg}", ex.Message);
            }
            return false;
        }

        public async Task<bool> DeleteInvalidRecord(int FileDetailId)
        {
            try
            {
                Context.FileErrorRecords.Where(x => x.IsActive && x.FileDetailId == FileDetailId)
                   .ExecuteUpdate(e => e.SetProperty(s => s.IsActive, s => !s.IsActive));
                await Context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in DeleteInvalidRecord :{ErrorMsg}", ex.Message);
            }
            return false;
        }

        public static List<KeyValuePair<string, string>> ConvertJsonToList(string jsonString)
        {
            JArray jsonArray = JArray.Parse(jsonString);
            List<KeyValuePair<string, string>> keyValuePairs = new List<KeyValuePair<string, string>>();

            foreach (var obj in jsonArray)
            {
                foreach (var property in ((JObject)obj).Properties())
                {
                    keyValuePairs.Add(new KeyValuePair<string, string>(property.Name, property.Value.ToString()));
                }
            }

            return keyValuePairs;
        }

        public List<DynamicDataTableModel> ListValidationByFileId(ValidationErrorListRequest request, out int totalCount)
        {
            List<DynamicDataTableModel> model = new List<DynamicDataTableModel>();
            totalCount = 0;
            try
            {
                var data = Context.FileErrorRecords.Where(x => x.IsActive && x.FileDetailId == request.FileDetailId && x.Status == (int)FileErrorRecordsStatus.Valided)
                    .AsNoTracking().ToList();
                if (data.Count > 0)
                {
                    totalCount = data.Count;
                    data = data.Skip((request.PageNo - 1) * request.PageSize).Take(request.PageSize).ToList();
                    foreach (var item in data)
                    {
                        DynamicDataTableModel dynamic = new DynamicDataTableModel();
                        dynamic.DynamicDataTable = ConvertJsonDictionary(item.FileRowData, item.ErrorMessage, item.Id.ToString());
                        model.Add(dynamic);
                        dynamic = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in ListValidationByFileId :{ErrorMsg}", ex.Message);
            }
            return model;
        }

        public async Task<List<FileDetailList>> GetAllFileDetails(FileDetailListRequest request)
        {
            List<FileDetailList> fileDetails = new List<FileDetailList>();
            try
            {
                fileDetails = await Context.UploadFileDetails.Where(x => x.IsActive && x.FacilityID == Convert.ToInt32(request.FacilityId)
                /*&& x.ReportQuarter == Convert.ToInt32(request.ReportQuarter) */
                && x.Year == Convert.ToInt32(request.Year))
                    .Select(s => new FileDetailList
                    {
                        FileNameId = s.Id,
                        FileName = s.FileName
                    }).ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in GetAllFileDetails :{ErrorMsg}", ex.Message);
            }
            return fileDetails;
        }

        public static Dictionary<string, object> ConvertJsonDictionary(string jsonString, string errorMessage, string ErrorId)
        {
            Dictionary<string, object> dataDictionary = new Dictionary<string, object>();

            JArray jsonArray = JArray.Parse(jsonString);
            JObject newKeyValuePair1 = new JObject(new JProperty("Error Message", errorMessage));
            JObject newKeyValuePair2 = new JObject(new JProperty("ErrorId", ErrorId));
            jsonArray.Add(newKeyValuePair1);
            jsonArray.Add(newKeyValuePair2);

            foreach (var item in jsonArray)
            {
                if (item is JObject jObject)
                {
                    foreach (var property in jObject.Properties())
                    {
                        dataDictionary[property.Name] = property.Value.ToObject<object>();
                    }
                }
            }

            return dataDictionary;
        }

        public async Task<UploadFileIdResponse> GetUploadFileDetailsByFileId(int fileId)
        {
            UploadFileIdResponse uploadFiles = new UploadFileIdResponse();
            try
            {
                GetQuarter getQuarter = new GetQuarter();
                List<QuarterDetails> quartersList = getQuarter.GetQuarterById();
                var query = (from u in Context.UploadFileDetails
                             join users in Context.AspNetUsers on u.CreatedBy equals users.Id
                             join f in Context.Facilities on u.FacilityID equals f.Id
                             where u.Id == fileId
                             select new
                             {
                                 u.FileName,
                                 FullName = users.FirstName + " " + users.LastName,
                                 u.CreatedDate,
                                 u.Year,
                                 u.ReportQuarter,
                                 f.FacilityName
                             }).AsEnumerable().Select(item => new UploadFileIdResponse
                             {
                                 FileName = item.FileName,
                                 UploadedBy = item.FullName,
                                 UploadedDate = Convert.ToDateTime(item.CreatedDate.ToString("MM-dd-yyyy HH:mm")),
                                 Year = item.Year,
                                 ReportQuarter = item.ReportQuarter.ToString(),
                                 FacilityName = item.FacilityName
                             }).ToList();

                if (query != null)
                {
                    uploadFiles = query[0];
                    uploadFiles.ReportQuarter = quartersList.Where(x => x.Id == Convert.ToInt32(uploadFiles.ReportQuarter)).Select(s => s.QuaterTitle).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetUploadFileDetailsByFileId :{ErrorMsg}", ex.Message);
            }
            return uploadFiles;
        }

        public List<ValidDataViewModel> GetFileValidRecordList(BackOutAllRecordRequest request, int pageSize, int pageNo, out int totalCount, string sortOrder)
        {
            List<ValidDataViewModel> validFileList = new List<ValidDataViewModel>();
            totalCount = 0;
            try
            {
                validFileList = (from ts in Context.Timesheet
                                 join f in Context.Facilities on ts.FacilityId equals f.Id
                                 join u in Context.UploadFileDetails on ts.FileDetailId equals u.Id
                                 where ts.IsActive && f.IsActive && u.IsActive && ts.FacilityId == request.FacilityId && ts.FileDetailId == request.FileDetailsId
                                 select new ValidDataViewModel
                                 {
                                     EmployeeId = ts.EmployeeId,
                                     FacilityId = ts.FacilityId,
                                     AgencyId = ts.AgencyId,
                                     ReportQuarter = ts.ReportQuarter,
                                     Month = ts.Month,
                                     Year = ts.Year,
                                     FirstName = ts.FirstName,
                                     LastName = ts.LastName,
                                     Workday = (ts.Workday == default(DateTime)) ? "NA" : ts.Workday.ToString("MM-dd-yyyy"),
                                     THours = ts.THours,
                                     JobTitleCode = ts.JobTitleCode,
                                     PayTypeCode = ts.PayTypeCode,
                                     FileDetailId = ts.FileDetailId,
                                     Status = ts.Status,
                                 }).ToList();

                if (validFileList.Count > 0)
                {
                    totalCount = validFileList.Count;
                    validFileList = validFileList.Skip(pageNo).Take(pageSize).ToList();
                    if (!string.IsNullOrEmpty(sortOrder))
                    {
                        switch (sortOrder)
                        {
                            case "employeeid_asc":
                                validFileList = validFileList.OrderBy(x => x.EmployeeId).ToList();
                                break;
                            case "employeeid_desc":
                                validFileList = validFileList.OrderByDescending(x => x.EmployeeId).ToList();
                                break;
                            case "firstname_asc":
                                validFileList = validFileList.OrderBy(x => x.FirstName).ToList();
                                break;
                            case "firstname_desc":
                                validFileList = validFileList.OrderByDescending(x => x.FirstName).ToList();
                                break;
                            case "lastname_asc":
                                validFileList = validFileList.OrderBy(x => x.LastName).ToList();
                                break;
                            case "lastname_desc":
                                validFileList = validFileList.OrderByDescending(x => x.LastName).ToList();
                                break;
                            case "pay type code_asc":
                                validFileList = validFileList.OrderBy(x => x.PayTypeCode).ToList();
                                break;
                            case "pay type code_desc":
                                validFileList = validFileList.OrderByDescending(x => x.PayTypeCode).ToList();
                                break;
                            case "work day_asc":
                                validFileList = validFileList.OrderBy(x => x.Workday).ToList();
                                break;
                            case "work day_desc":
                                validFileList = validFileList.OrderByDescending(x => x.Workday).ToList();
                                break;
                            case "hours_asc":
                                validFileList = validFileList.OrderBy(x => x.THours).ToList();
                                break;
                            case "hours_desc":
                                validFileList = validFileList.OrderByDescending(x => x.THours).ToList();
                                break;
                            case "job title code_asc":
                                validFileList = validFileList.OrderBy(x => x.JobTitleCode).ToList();
                                break;
                            case "job title code_desc":
                                validFileList = validFileList.OrderByDescending(x => x.JobTitleCode).ToList();
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in GetFileErrorList :{ErrorMsg}", ex.Message);
            }
            return validFileList;
        }

        public DataTable GetInvalidErrorFileDataForCsv(int facilityId, int reportQuarter, int year, int fileDetailsId)
        {
            List<FileErrorList> fileErrorList = new List<FileErrorList>();
            List<FileErrorList> updatedFileErrorList = new List<FileErrorList>();
            DataTable dataTable = new DataTable();
            try
            {
                fileErrorList = (from fe in Context.FileErrorRecords
                                 join f in Context.Facilities on fe.FacilityId equals f.Id
                                 join u in Context.UploadFileDetails on fe.FileDetailId equals u.Id
                                 where fe.IsActive && f.IsActive && u.IsActive && fe.FacilityId == facilityId && fe.FileDetailId == fileDetailsId && fe.Status == 0
                                 select new FileErrorList
                                 {
                                     Id = fe.Id,
                                     ErrorMessage = fe.ErrorMessage,
                                     FileRowData = fe.FileRowData,
                                     FacilityId = fe.FacilityId,
                                     FacilityName = f.FacilityName,
                                     Year = fe.Year,
                                     ReportQuarter = fe.ReportQuarter,
                                     Month = fe.Month,
                                     FileDetailId = fe.FileDetailId,
                                     FileName = u.FileName
                                 }).ToList();

                foreach (var fileError in fileErrorList)
                {
                    var jsonArray = JsonConvert.DeserializeObject<JArray>(fileError.FileRowData);

                    FileRowData fileRowData = new FileRowData();
                    foreach (var jsonObject in jsonArray)
                    {
                        var property = (JProperty)jsonObject.First;
                        var propertyName = property.Name;
                        var propertyValue = property.Value;

                        if (propertyName == "Employee Id") fileRowData.EmployeeId = propertyValue.ToString();
                        else if (propertyName == "Pay Type Code") fileRowData.PayTypeCode = Convert.ToInt32(propertyValue.ToString());
                        else if (propertyName == "Work Day" || propertyName == "Workday") fileRowData.WorkDay = propertyValue.ToString();
                        else if (propertyName == "Hours") fileRowData.Hours = Convert.ToInt32(propertyValue.ToString());
                        else if (propertyName == "Job Title Code") fileRowData.JobTitleCode = Convert.ToInt32(propertyValue.ToString());
                        else if (propertyName == "First Name" || propertyName == "Name") fileRowData.EmployeeFirstName = propertyValue.ToString();
                    }

                    FileErrorList tempFileErrorList = new FileErrorList();
                    tempFileErrorList.finalFileRowData = fileRowData;
                    tempFileErrorList.Id = fileError.Id;
                    tempFileErrorList.ErrorMessage = fileError.ErrorMessage;
                    tempFileErrorList.FileRowData = fileError.FileRowData;
                    tempFileErrorList.FacilityId = fileError.FacilityId;
                    tempFileErrorList.FacilityName = fileError.FacilityName;
                    tempFileErrorList.Year = fileError.Year;
                    tempFileErrorList.ReportQuarter = fileError.ReportQuarter;
                    tempFileErrorList.Month = fileError.Month;
                    tempFileErrorList.FileDetailId = fileError.FileDetailId;
                    tempFileErrorList.FileName = fileError.FileName;

                    updatedFileErrorList.Add(tempFileErrorList);

                }

                dataTable.Columns.Clear();
                dataTable.Columns.Add(new DataColumn("ErrorMessage"));
                dataTable.Columns.Add(new DataColumn("EmployeeId"));
                dataTable.Columns.Add(new DataColumn("EmployeeFirstName"));
                dataTable.Columns.Add(new DataColumn("PayTypeCode"));
                dataTable.Columns.Add(new DataColumn("WorkDay"));
                dataTable.Columns.Add(new DataColumn("Hour"));
                dataTable.Columns.Add(new DataColumn("JobTitleCode"));
                foreach (var item in updatedFileErrorList)
                {
                    var dataRow = dataTable.NewRow();
                    dataRow["ErrorMessage"] = item.ErrorMessage;
                    dataRow["EmployeeId"] = item.finalFileRowData.EmployeeId;
                    dataRow["EmployeeFirstName"] = item.finalFileRowData.EmployeeFirstName;
                    dataRow["PayTypeCode"] = item.finalFileRowData.PayTypeCode;
                    dataRow["WorkDay"] = item.finalFileRowData.WorkDay;
                    dataRow["Hour"] = item.finalFileRowData.Hours;
                    dataRow["JobTitleCode"] = item.finalFileRowData.JobTitleCode;
                    dataTable.Rows.Add(dataRow);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetInvalidErrorFileDataForCsv:{ErrorMsg}", ex.Message);
            }
            return dataTable;
        }

        public DataTable GetValidFileDataForCsv(int facilityId, int fileDetailsId)
        {
            List<ValidDataViewModel> validFileList = new List<ValidDataViewModel>();
            DataTable dataTable = new DataTable();
            try
            {
                validFileList = (from ts in Context.Timesheet
                                 join f in Context.Facilities on ts.FacilityId equals f.Id
                                 join u in Context.UploadFileDetails on ts.FileDetailId equals u.Id
                                 where ts.IsActive && f.IsActive && u.IsActive && ts.FacilityId == facilityId && ts.FileDetailId == fileDetailsId
                                 select new ValidDataViewModel
                                 {
                                     EmployeeId = ts.EmployeeId,
                                     FacilityId = ts.FacilityId,
                                     AgencyId = ts.AgencyId,
                                     ReportQuarter = ts.ReportQuarter,
                                     Month = ts.Month,
                                     Year = ts.Year,
                                     FirstName = ts.FirstName,
                                     LastName = ts.LastName,
                                     Workday = (ts.Workday == default(DateTime)) ? "NA" : ts.Workday.ToString("MM-dd-yyyy"),
                                     THours = ts.THours,
                                     JobTitleCode = ts.JobTitleCode,
                                     PayTypeCode = ts.PayTypeCode,
                                     FileDetailId = ts.FileDetailId,
                                     Status = ts.Status,
                                 }).ToList();

                dataTable.Columns.Clear();
                dataTable.Columns.Add(new DataColumn("EmployeeId"));
                dataTable.Columns.Add(new DataColumn("EmployeeFirstName"));
                dataTable.Columns.Add(new DataColumn("PayTypeCode"));
                dataTable.Columns.Add(new DataColumn("WorkDay"));
                dataTable.Columns.Add(new DataColumn("Hour"));
                dataTable.Columns.Add(new DataColumn("JobTitleCode"));
                foreach (var item in validFileList)
                {
                    var dataRow = dataTable.NewRow();
                    dataRow["EmployeeId"] = item.EmployeeId;
                    dataRow["EmployeeFirstName"] = item.FirstName;
                    dataRow["PayTypeCode"] = item.PayTypeCode;
                    dataRow["WorkDay"] = item.Workday;
                    dataRow["Hour"] = item.THours;
                    dataRow["JobTitleCode"] = item.JobTitleCode;
                    dataTable.Rows.Add(dataRow);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetValidErrorFileDataForCsv:{ErrorMsg}", ex.Message);
            }
            return dataTable;
        }

        public async Task<FileDetailList> GetFileDetails(FileDetailListRequest request)
        {
            var fileDetails = new FileDetailList();
            try
            {
                fileDetails = Context.UploadFileDetails.Where(x => x.IsActive && x.FacilityID == Convert.ToInt32(request.FacilityId)
                && x.ReportQuarter == Convert.ToInt32(request.ReportQuarter) && x.Year == Convert.ToInt32(request.Year) && x.Id == Convert.ToInt32(request.FileDetailsId))
                    .Select(s => new FileDetailList
                    {
                        FileNameId = s.Id,
                        FileName = s.FileName
                    }).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in GetAllFileDetails :{ErrorMsg}", ex.Message);
            }
            return fileDetails;
        }

        public DataTable GetInvalidErrorFileDynamicDataForCsv(int fileDetailsId)
        {
            List<DynamicDataTableModel> model = new List<DynamicDataTableModel>();
            DataTable resultDataTable = new DataTable();

            try
            {
                var data = Context.FileErrorRecords.Where(x => x.IsActive && x.FileDetailId == fileDetailsId)
                    .AsNoTracking().ToList();

                if (data.Count > 0)
                {
                    foreach (var item in data)
                    {
                        DynamicDataTableModel dynamic = new DynamicDataTableModel();
                        dynamic.DynamicDataTable = ConvertJsonDictionary(item.FileRowData, item.ErrorMessage, item.Id.ToString());
                        model.Add(dynamic);
                        dynamic = null;
                    }

                    // Convert the list of DynamicDataTableModel to DataTable
                    resultDataTable = ConvertToDataTable(model);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in GetInvalidErrorFileDynamicDataForCsv :{ErrorMsg}", ex.Message);
            }
            return resultDataTable;
        }

        public DataTable ConvertToDataTable(List<DynamicDataTableModel> modelList)
        {
            DataTable dataTable = new DataTable();

            // Assuming DynamicDataTableModel has properties named DynamicDataTable, you can access them like this
            if (modelList.Count > 0)
            {
                foreach (var key in modelList[0].DynamicDataTable.Keys)
                {
                    dataTable.Columns.Add(key);
                }

                foreach (var model in modelList)
                {
                    DataRow row = dataTable.NewRow();
                    foreach (var entry in model.DynamicDataTable)
                    {
                        row[entry.Key] = entry.Value;
                    }
                    dataTable.Rows.Add(row);
                }
            }

            return dataTable;
        }

        public string GetFacilityName(int facilityId)
        {
            var facilityName = "";
            try
            {
                var result = Context.Facilities.Where(f => f.Id == facilityId).FirstOrDefault();
                if (result != null)
                {
                    return result.FacilityName;
                }
                else
                {
                    return facilityName;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Upload File Repository / Getting Facility Name:{ErrorMsg}", ex.Message);
                return facilityName;
            }
        }

        public async Task<List<ValidDataViewModel>> GetWorkDayListByEmployee(EmployeeWorkDayRequest request)
        {
            List<ValidDataViewModel> model = new List<ValidDataViewModel>(); 
            try
            {
                if(request != null && request.EmployeeId != null)
                {
                    model = await Context.Timesheet.AsNoTracking().Where(x => x.IsActive && x.EmployeeId == request.EmployeeId
                    && x.Workday.Date == Convert.ToDateTime(request.WorkDay).Date && x.FacilityId == request.FacilityId).Select(s => new ValidDataViewModel
                    {
                        EmployeeId = s.EmployeeId,
                        Workday = s.Workday.ToString("MM-dd-yyyy"),
                        THours = s.THours,
                        PayTypeCode = s.PayTypeCode,
                        JobTitleCode = s.JobTitleCode,
                        FirstName = (s.FirstName == null) ? "" : s.FirstName,
                        LastName = (s.LastName == null) ? "" : s.LastName,
                    }).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in GetWorkDayListByEmployee", ex.Message);
            }
            return model;
        }
    }
}
