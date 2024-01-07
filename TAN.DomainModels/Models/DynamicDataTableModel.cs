using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Models
{
    [ExcludeFromCodeCoverage]
    public class DynamicDataTableModel
    {
        public Dictionary<string, object>? DynamicDataTable { get; set; }
    }
    [ExcludeFromCodeCoverage]
    public class ValidationErrorListResponse
    {
        public List<DynamicDataTableModel> dynamicDataTableModel { get; set; }
        public int TotalCount { get; set; }
        public int RowCount { get; set; } = 0;
        public int RowStart { get; set; } = 0;
        public UploadFileIdResponse uploadFileDetails { get; set; }
    }

    public class UploadFileIdResponse
    {
        public string? FileName { get; set;}
        public string? UploadedBy { get; set;}
        public DateTime UploadedDate { get; set;}
        public int Year { get; set;}
        public string? ReportQuarter { get; set;}
        public string? FacilityName { get; set;}
    }
}
