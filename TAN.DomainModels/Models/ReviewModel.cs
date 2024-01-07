using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;

namespace TAN.DomainModels.Models
{
    public class ReviewModel
    {
        public List<FacilityModel> FacilityList { get; set; }
        [ExcludeFromCodeCoverage]
        public List<EmployeeData> EmployeeList { get; set; }
        [ExcludeFromCodeCoverage]
        public List<StaffingDepartmentDetails> SatffingDepartmentLIst { get; set; }
        [ExcludeFromCodeCoverage]
        public CensusViewModel CensusModel { get; set; }
        [ExcludeFromCodeCoverage]
        public StaffingDepartmentModel StaffingDepartment { get; set; }
        public EmployeeModel EmployeeDetailModel { get; set; }
        public string? FileId { get; set; } = "";
        public string? FacilityId { get; set; } = "";
    }

    [ExcludeFromCodeCoverage]
    public class CensusData
    {
        public string? CensusId { get; set; }
        public string? FacilityName { get; set; }
        public int Year { get; set; }
        public string Month { get; set; }
        public string? MonthName { get; set; }
        public int Medicare { get; set; }
        public int Medicad { get; set; }
        public int Other { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class CensusViewModel
    {
        public Pager CensusPager { get; set; }
        public List<CensusData> CensusData { get; set; } = new List<CensusData>();
    }

    [ExcludeFromCodeCoverage]
    public class CensusViewRequest
    {
        public string FacilityID { get; set; }
        public int Year { get; set; }
        public int ReportQuarter { get; set; }
        public string? SearchValue { get; set; }
    }

    public class CensusCsvRequest
    {
        public int FacilityID { get; set; }
        public int Year { get; set; }
        public int ReportQuarter { get; set; }
        public string? FileName { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class SearchResponse
    {
        public int Id { get; set; }
        public string? Text { get; set; }
    }

    public class Response
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
    }

    public class EmployeeById
    {
        public string? Id { get; set; }
        public string? FullName { get; set; }

        public string? PayTypeName { get; set; }
        public string? JobTitle { get; set; }
        public string? FacilityName { get; set;}
        public string? OrganizationName { get; set; }
    }
}
