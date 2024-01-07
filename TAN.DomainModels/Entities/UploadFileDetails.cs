using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Entities
{
    [ExcludeFromCodeCoverage]
    public class UploadFileDetails
    {
        public UploadFileDetails() { }

        [Key]
        public int Id { get; set; }
        public int FacilityID { get; set; } 
        public string FileName { get; set; }
        public int RowCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public int ReportQuarter { get;set; }
        public int Year { get;set; }
        public int Month { get;set; }
    }
}
