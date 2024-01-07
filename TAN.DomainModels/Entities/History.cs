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
    public class History
    {
        [Key]
        public int Id { get; set; }
        public int FacilityId { get; set; }
        public int ReportQuarter { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string? FileName { get; set; }
        public int Status { get; set; } 
        public DateTime CreateDate { get; set; }
        public string? CreateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public string? UpdateBy { get; set; }
        public bool IsActive { get; set; }
        public int OrganizationId { get;set; }
    }
}
