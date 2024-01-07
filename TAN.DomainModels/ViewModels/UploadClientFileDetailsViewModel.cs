using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.ViewModels
{
    public class UploadClientFileDetailsViewModel
    {
        public int? FacilityId { get; set; }
        public int? OrganizationId { get; set; }
        public string? FileName { get; set; }
        public string? FileSize { get; set; }
        public string? CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public List<Itemlist>? OrganizationList { get; set; }
        public List<Itemlist>? CarrierList { get; set; }
        public List<Itemlist>? FacilityList { get; set; }
        public int? UserType { get; set; }
        public string? FacilityName { get; set; }
        public string? OrganizationName { get; set; }
        public string? CarrierName { get; set; }
        public int IsProcessed { get; set; }
    }
}
