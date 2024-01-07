using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;

namespace TAN.DomainModels.ViewModels
{
    public class SubmissionHistoryViewModel
    {
        public List<Itemlist>? OrganizationList { get; set; }
        public List<FacilityModel>? FacilityList { get; set; }

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
        public string SubmissionDate { get;set; }
        public int OrganizationId { get;set; }
        public string? FacilityName { get; set; }
        public string? OrganizationName { get; set; }
        public int UserType { get; set; }

    }
}
