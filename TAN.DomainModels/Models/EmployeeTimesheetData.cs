using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TAN.DomainModels.Models.XMLReaderModel;
using TAN.DomainModels.Entities;
using System.Numerics;
using System.Diagnostics.CodeAnalysis;

namespace TAN.DomainModels.Models
{
    public class EmployeeTimesheetData
    {
        public DateTime Workday { get; set; }
        public int TimesheetId {get;set;}
        public string? EmployeeId {get;set;}
        public int FacilityId {get;set;}
        public string? TotalHours {get;set;}
        public bool Editable { get; set; } = true;
        public string? Color { get; set; }
        public bool IsButtonEditable { get; set; } = true;
    }

    public class EmployeeTimesheetDataRequest
    {
        [ExcludeFromCodeCoverage]
        public string? TimesheetId { get; set; }
        public string? EmployeeId { get; set; }
        public int FacilityId { get; set; }
        public int ReportQuarter { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string? Workday { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now;
    }
    public class EmployeeTimesheetSaveDataRequest
    {
        public string? TimesheetId { get; set; }
        public string? EmployeeId { get; set; }
        public int FacilityId { get; set; }
        public int ReportQuarter { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int PayCode { get; set; }
        public int JobTitle { get; set; }
        public int UploadType { get; set; }
        public string? TotalHours { get; set;}
        public string? WorkDate { get; set; }
        public List<string>? WorkDateList { get; set; }
    }

    public class EmployeeMultiDatesSaveRequest
    {
        public string? EmployeeId { get; set; }
        public int FacilityId {get;set;}
        public int Year {get;set;}
        public int ReportQuarter {get;set;}
        public int Month {get;set;}
        public int PayCode {get;set;}
        public int JobTitle {get;set;}
        public string? WorkingHours {get;set;}
        public List<string>? MultipleDates {get;set;}
    }

    public class ApproveTimesheetRequest
    {
        public int FacilityId { get; set; }
        public int Year { get; set; }
        public int ReportQuarter { get; set; }
        public int Month { get; set; }
        public string? UserId { get; set; } = "";
        public bool IsApproved { get; set; }
    }

    public class NursesMissingDates
    {
        public int FacilityId { get; set; }
        public int Year { get; set; }
        public int ReportQuarter { get; set; }
        public int Month { get; set; }
        public string? JobTitle { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now;
    }

    public class NursesMissingDatesResponse
    {
        public DateTime MissingDate { get; set; }
        public bool IsEditable { get; set; } = false;
    }

    public class ApproveTimesheetResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        
    }

    public class DeleteTimesheetDataRequest
    {
        public string? TimesheetId { get; set; }
        public string? UserID { get; set; }
    }
}
