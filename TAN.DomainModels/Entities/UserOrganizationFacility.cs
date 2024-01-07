using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Entities
{
    public class UserOrganizationFacility
    {
        [Key]
        public int Id { get; set; }
        [ExcludeFromCodeCoverage]
        public AspNetUser AspNetUser { get; set; }
        [ForeignKey("AspNetUser")]
        public string UserId { get; set; }
        [ExcludeFromCodeCoverage]
        public OrganizationModel Organization { get; set; }
        [ForeignKey("OrganizationModel")]
        public int OrganizationID { get; set; }
        [ExcludeFromCodeCoverage]
        public FacilityModel Facility { get; set; }
        [ForeignKey("FacilityModel")]
        public int FacilityID { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
    }
}
