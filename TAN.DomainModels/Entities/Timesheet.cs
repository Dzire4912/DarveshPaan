using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Helpers;

namespace TAN.DomainModels.Entities
{
    [ExcludeFromCodeCoverage]
    [SkipAudit]
    public class Timesheet
    {
        [Key]
        public Int64 Id { get; set; }
        public string? EmployeeId { get; set; }
        public int FacilityId { get; set; }
        public int AgencyId { get; set; }
        public int ReportQuarter { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int UploadType { get; set; }
        public DateTime Workday { get; set; }
        public decimal THours { get; set; }
        public int JobTitleCode { get; set; }
        public int PayTypeCode { get; set; }  
        public int FileDetailId { get; set; } 
        public int Status { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string? Createby { get; set; }
        public DateTime UpdateDate { get; set; }
        public string? UpdateBy { get; set; }
        public bool IsActive { get; set; } = true;
    }

    [ExcludeFromCodeCoverage]
    [SkipAudit]
    public class TimesheetEmployeeDataRequest
    {
        public List<string>? EmployeeId { get; set; }
        public int FacilityId { get; set; }
        public int ReportQuarter { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public DateTime Workday { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now;
    }

    [ExcludeFromCodeCoverage]
    [SkipAudit]
    public class TimesheetEmployeeDataResponse
    {
        public string? EmployeeId { get; set; }
        public int FacilityId { get; set; } 
        public DateTime Workday { get; set; }
        public decimal THours { get; set; }
    }
}
