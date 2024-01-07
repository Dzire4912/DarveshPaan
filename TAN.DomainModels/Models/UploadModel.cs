using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using TAN.DomainModels.Entities;

namespace TAN.DomainModels.Models
{
    public class UploadModel
    {
        public class RequestModel
        {
            public int FacilityId { get; set; }
            public string? FacilityName { get; set; }
            public int year { get; set; }
            public int Quarter { get; set; }
            public int Month { get; set; }
            public string? UploadType { get; set; }
            public string? AgencyId { get; set; }
            public string? Filename { get; set; }
            public int FileNameId { get; set; }
            public string? FileURL { get; set; }
            public bool IsOneDrive { get; set; }
        }

        public class ResponseUploadModel
        {
            public int StatusCode { get; set; }
            public string? Message { get; set; }
            public DataTable? previewTable { get; set; }
            public DataTable? UploadFileData { get; set; }
            public List<CMSMappingFieldData>? CmsMappingFieldDataList { get; set; }
        }

        public class ResponseMapping
        {
            public int StatusCode { get; set; }
            public string? Message { get; set; }
            public List<Timesheet> DiffQuarterTimesheet { get; set; } = new List<Timesheet>();
            public string? URL { get; set; }
        }

        public class UploadView
        {
            public List<FacilityModel> FacilityList { get; set; }
            public int FacilityId { get; set; }
        }

        public class MappingRequestData
        {
            public List<MappingData>? MappingDatas { get; set; }
            public string? RememberMe { get; set; }
            public string? FacilityId { get; set; }  
            public string? Filename { get; set; }
            public int FileNameId { get; set; }
            public RequestModel requestModel { get; set; }

        }

        public class MappingData
        {
            public int CmsMappingID { get; set; }
            public string? ColumnName { get; set; }
            public string? CmsFieldName { get; set; }
        }
         
        public class UploadFileDetailsWithFilter
        {
            public DataTable? PreviewTableData { get; set; }
            public DataTable? FileDataTable { get; set; }
            public  List<CMSMappingFieldData>? DtCmsList { get; set; }
            public List<TimesheetEmployeeDataResponse>? TimesheetResponsesList { get; set; }
            public int FacilityId { get; set;}
            public int Year { get; set;}
            public int Month { get; set;}
            public int Quarter { get; set;}
            public int UploadType { get; set;}
            public string? FileName { get; set; }
            public int FileNameId { get; set; }
            public List<PayTypeCodes>? PayTypeCode { get; set; }
            public List<JobCodes>? JobCode { get; set; }
            public List<Employee>? ValidEmployeeList { get; set; }
            public Stream File { get; set; }
            public List<Timesheet> DiffQuarterTimesheet { get; set; } = null;
            public bool IsAllRecordInserted { get; set; } = false;

    }

        public class ValidatDataResponse
        {
            public bool? Success { get; set; }
            public string? ErrorMessage { get; set; }
        }

        public class EmployeeWorkDayRequest
        {
            public string? EmployeeId { get; set; }
            public int FacilityId { get; set; }
            public DateTime WorkDay { get; set; }
        }
    }
}
