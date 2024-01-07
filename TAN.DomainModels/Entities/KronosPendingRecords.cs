using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Entities
{
    public class KronosPendingRecords
    {
        [Key]
        public int rowid {  get; set; }
        public string EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public int FacilityId { get; set; }
        public string? FacilityName { get; set; }
        public DateTime WorkDay { get; set; }
        public int OrgId { get; set; } = 0;


    }
}
