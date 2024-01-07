using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;

namespace TAN.DomainModels
{
    [ExcludeFromCodeCoverage]
    public class BreakDeduction
    {
        [Key]
        public int Id {  get; set; }
        public FacilityModel Facility { get; set; }

        [ForeignKey("Facility")]
        public int FacilityId { get; set; }
        public int RegularTime { get; set;}
        public int OverTime { get; set;}
        public string? CreatedBy {  get; set; } 
        public DateTime? CreatedTime { get; set;}
        public DateTime? UpdatedTime { get; set; }
    }
}
