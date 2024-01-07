using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.ViewModels
{
    public class ReportVerificationViewModel
    {
        public string? ValidatedBy { get; set; }
        public string? ValidationDate { get; set; }

        public string? ApprovedBy { get; set; }
        public string? ApprovedDate { get; set; }
    }

    public class ValidationResult
    {
        public bool Success { get; set; }
        public string FullName { get; set; }
        public string ValidationDate { get; set; }
        public string ApprovedDate { get; set; }
        public string Message { get; set; }
    }
}
