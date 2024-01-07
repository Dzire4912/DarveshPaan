using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using static TAN.DomainModels.Models.XMLReaderModel;
using TAN.DomainModels.ViewModels;

namespace TAN.DomainModels.Models
{
    public class EmployeeData
    {
        public int Id { get; set; }
        public string? EmployeeId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PayType { get; set; }
        public string? JobTitle { get; set; }
        public string? HireDate { get; set; }
        public string? TerminationDate { get; set; }
        public string? FacilityId { get; set; }
        [ExcludeFromCodeCoverage]
        public string? FacilityName { get; set; }
        [ExcludeFromCodeCoverage]
        public string? CreateDate { get; set; }
        [ExcludeFromCodeCoverage]
        public string? EmployeeFullName { get; set; }
        [ExcludeFromCodeCoverage]
        public bool IsActive { get; set; }
    }

    public class EmployeeModel
    {
        public IEnumerable<EmployeeData> EmployeeDataList { get; set; } = new List<EmployeeData>();
        public IEnumerable<CustomEmployeeData> CustomEmployeeDataList { get; set; } = new List<CustomEmployeeData>();
        public EmployeePopupModel? employeePopupModel { get; set; }
    }

    public class EmployeePopupModel
    {
        public List<PayTypeCodes> PayType { get; set; } = new List<PayTypeCodes>();
        public List<JobCodes> JobCode { get; set; } = new List<JobCodes>();
        public List<FacilityModel>? FacilityList { get; set; } = new List<FacilityModel>();
    }

    public class EmployeeViewModel
    {
        public List<FacilityModel>? FacilityList { get; set; }
        public EmployeeModel EmployeeDetailModel { get; set; }
    }
    [ExcludeFromCodeCoverage]
    public class ImportFileResponse
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public List<KeyValuePair<string, string>> keyValuePairs { get; set; } = new List<KeyValuePair<string, string>>();
    }

    public class EmployeeExportRequest
    {
        public string? PayType { get; set; }
        public string? JobTitle { get; set; }
        public string? FormatType { get; set; }
        public string? Quarter { get; set; }
        public string? Year { get; set; }
        public string? Month { get; set; }
        public string? FacilityId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Filename { get; set; }
        public string? EmployeeIds { get; set; } 
    }

    public class StaffExportList
    {
        public List<StaffExport>? staffExports { get; set; }
        public List<EmployeeData>? EmployeeData { get; set; }
    }

    public class StaffExport
    {
        public string? EmployeeId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? HireDate { get; set; }
        public string? TerminationDate { get; set; }
        public string? PayTypeDescription { get; set; }
        public string? JobTitle { get; set; }
        public string? Workday { get; set; }
        public string? THours { get; set; }
        public string? HoursPayType { get; set; }
        public string? HoursJobTitle { get; set; }

    }
    [ExcludeFromCodeCoverage]
    public class EmployeeListRequest
    {
        public string? FacilityId { get; set; }
        public string? ReportQuarter { get; set; }
        public string? Year { get; set; }
        public string? Month { get; set; }
        public string? SearchValue { get; set; }
    }

    public class GetEmployeeDetailsRequest
    {
        public string? FacilityId { get; set; }
        public string? EmployeeId { get; set; }
    }

    public class CustomEmployeeData
    {
        public int Id { get; set; }
        public string? EmployeeId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PayType { get; set; }
        public int PayTypeCode { get; set; }
        public string? JobTitle { get; set; }
        public int JobTitleCode { get; set; }
        public string? HireDate { get; set; }
        public string? TerminationDate { get; set; }
        public string? FacilityId { get; set; }
        public string? FacilityName { get; set; }
        public string? CreateDate { get; set; }
        public string? EmployeeFullName { get; set; }
        public bool IsActive { get; set; }
    }
}