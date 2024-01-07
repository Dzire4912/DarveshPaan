using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Models
{
    [ExcludeFromCodeCoverage]
    public class StaffingDepartmentDetails
    {
        public string? Title { get; set; }
        public string? TotalHours { get; set; }
        public string? StaffCount { get; set; } 
        public string? Exempt { get; set; }
        public string? NonExempt { get; set; }
        public string? Contractors { get; set; }
        public string? FacilityName { get; set; }
        public int Id { get; set; }
        public string? FacilityId { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class StaffingDepartmentModel
    {
        public Pager StaffDeptPager { get; set; }
        public List<StaffingDepartmentDetails> SatffingDepartmentList { get; set; } = new List<StaffingDepartmentDetails>();
    }

    [ExcludeFromCodeCoverage]
    public class StaffingDepartmentRequest
    {
        public int PageNo { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int Facility { get; set; }
    }

    public class StaffingDepartmentCsvRequest
    {
        public int FacilityID { get; set; }
        public int Year { get; set; }
        public int ReportQuarter { get; set; }
        public string? FileName { get; set; }
    }
}
