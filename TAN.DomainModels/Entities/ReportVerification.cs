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
    public class ReportVerification
    {
        [Key]
        public int Id { get; set; }
        public DateTime ValidationDate { get; set; }
        public DateTime ApprovedDate { get; set; }
        public string? ValidationUserId { get; set; }
        public string? ApprovedByUserId { get; set; }
        public int FacilityId { get; set; }
        public int Year { get; set; }
        public int Quarter { get; set; }
        public int Month { get; set; }
        public string? ValidationStatus { get; set; }
        public string? ApprovedStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
    }
}
