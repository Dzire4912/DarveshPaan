using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.ViewModels
{
    public class KronosComarisionVM
    {
        public string Id { get; set; }
        public string EmployeeId { get; set; }
        public string? FacilityName { get; set; }
        public int facilityId { get; set; }
        public DateTime WorkDay { get; set; }     
        public string? PayType { get; set; }
        public int PayTypeCode { get; set; }
        public string? JobType { get; set; }
        public int JobTypeCode { get; set; }
        public decimal Hours { get; set; }
        public string Source { get; set; }
       


    }
}
