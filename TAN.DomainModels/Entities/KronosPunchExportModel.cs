using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TAN.DomainModels.Helpers;

namespace TAN.DomainModels.Entities
{
    [SkipAudit]
    public class KronosPunchExportModel
    {
        [Key]
        public int Id { get; set; }
        public string facilityId { get; set; }
        public string EmployeeID { get; set; }
        public string? EmployeeName { get; set; }
        public string? NATIVEKRONOSID { get; set; }
        public string? DEPARTMENTNUMBER { get; set; }
        public string? DepartmentDescription { get; set; }
        public string? JOBCODE { get; set; }
        public string? JobDescription { get; set; }
        public DateTime? ADJUSTEDAPPLYDATE { get; set; }
        public string? PayCode { get; set; }
        public decimal? TIMEINHOURS { get; set; }
        public string HrJob { get; set; }

        [ForeignKey("Organizations")]
        public int OrganizationId {  get; set; }    
        public OrganizationModel Organizations { get; set;}

    }
}
