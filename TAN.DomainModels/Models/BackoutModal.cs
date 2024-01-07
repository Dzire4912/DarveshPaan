using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using TAN.DomainModels.ViewModels;

namespace TAN.DomainModels.Models
{
    [ExcludeFromCodeCoverage]
    public class BackoutModal
    {
        public List<FacilityModel> FacilityList { get; set; }
        public List<UploadFileDetails> UploadList { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class FileDetailsResponse
    {
        public string? FileName { get; set; }
        public int FileDetailsId { get; set; }
        public int InvalidRecord { get; set; }
        public int ValidRecord { get; set; }
        public int RowCount { get; set; }
        public string? CreateDate { get; set; }
        public string? FacilityID { get; set; }
        public string? FacilityName { get; set; }
        public int Year { get; set; }
        public int Month{ get; set; }
        public int ReportQuarter{ get; set; }

    }

    public class BackOutAllRecordRequest
    {
        public int FileDetailsId { get; set; }
        public int FacilityId { get; set; }
    }
    public class BackOutAllRecordResponse
    {
        public bool status { get; set; }
        public string? Message { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class FileErrorList
    {
        public int Id { get; set; }
        public string? ErrorMessage { get; set; }
        public string? FileRowData { get; set; }
        public int FileDetailId { get; set; }
        public int FacilityId { get; set; }
        public string? FacilityName { get; set; }
        public string? FileName { get; set; }
        public int ReportQuarter { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public List<KeyValuePair<string, string>>? keyValueList { get; set; }

        public FileRowData? finalFileRowData { get; set; }
    }

    public class ErrorData
    {
        public int Id { get; set; }
        public int FileDetailId { get; set; }
        public int FacilityId { get; set; }
        public int ReportQuarter { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int UploadType { get; set; }
        public List<KeyValuePair<string, string>>? keyValueList { get; set; }
        public List<ValidDataViewModel>? WorkdayList { get; set; } = new List<ValidDataViewModel>();

    }

    [ExcludeFromCodeCoverage]
    public class BackoutTableRequest
    { 
        public int FacilityID { get; set; }
        public int Year { get; set; }
        public int ReportQuarter { get; set; }
        public string? Search { get; set; }
        public int Status { get; set; }
    }
    [ExcludeFromCodeCoverage]
    public class FileRowData
    {
        public string? EmployeeId { get; set; }
        public int PayTypeCode { get; set; }
        public string? WorkDay { get; set; }
        public int JobTitleCode { get; set; }
        public string? EmployeeFirstName { get; set; }
        public int Hours { get; set;}
    }
    [ExcludeFromCodeCoverage]
    public class ValidationErrorListRequest
    {
        public int FileDetailId { get; set; }
        public int PageNo { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; } = 0;
        public int RowCount { get; set; } = 0;
    }
}

