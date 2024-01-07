using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Entities
{
    public class KronosOneTimeUpdate
    {
        [Key]
        public int Id { get; set; }     
        public FacilityModel Facility { get; set; }
        [ForeignKey("Facility")]
        public int FacilityId { get; set; }
        public string EmployeeId { get; set;}
        public DateTime WorkDay { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } 
       

    }
}
