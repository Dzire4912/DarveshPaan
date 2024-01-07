using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Entities
{
    [ExcludeFromCodeCoverage]
    public class FileErrorRecords
    {
        public int Id { get; set; }
        public string? FileName { get; set; }
        public string? ErrorMessage { get; set; }
        public string? FileRowData { get; set; }
        public int FacilityId { get; set; }
        public int ReportQuarter { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int UploadType { get; set; } 
        public int FileDetailId { get; set; }
        public int Status { get; set; } 
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? Createdby { get; set; }
        public DateTime UpdateDDate { get; set; }
        public string? UpdateDBy { get; set; }
        public bool IsActive { get; set; } = true;
    }
    [ExcludeFromCodeCoverage]
    public class FileDetailList
    {
        public int FileNameId { get; set; }
        public string? FileName { get; set; }
    }
    [ExcludeFromCodeCoverage]
    public class FileDetailListRequest
    {
        public int FacilityId { get; set; }
        public int ReportQuarter { get; set; }
        public int Year { get; set; }
        public int FileDetailsId { get; set; }
    }
}
