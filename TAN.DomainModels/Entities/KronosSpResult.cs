using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Entities
{
    public class KronosSpResult
    {
       
        public string EmployeeId { get; set; }
        public int PayTypeCode { get; set; }
        public int JobTitleCode { get; set; }            
        public string? Workday { get; set; }
        public decimal THours { get; set; }
        public int FacilityId {  get; set; }
        public string FullName { get; set; }

    }
}
