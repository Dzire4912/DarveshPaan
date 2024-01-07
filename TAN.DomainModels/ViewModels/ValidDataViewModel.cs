using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.ViewModels
{
    public class ValidDataViewModel
    {
        public string? EmployeeId { get; set; }
        public int FacilityId { get; set; }
        public int AgencyId { get; set; }
        public int ReportQuarter { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Workday { get; set; }
        public decimal THours { get; set; }
        public int JobTitleCode { get; set; }
        public int PayTypeCode { get; set; }
        public int FileDetailId { get; set; }
        public int Status { get; set; }
    }
}
