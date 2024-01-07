using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace TAN.DomainModels.Entities
{
    public class FacilityModel
    {
        [Key]
        public int Id { get; set; }
        public string? FacilityID { get; set; }
        public string? FacilityName { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        [ForeignKey("Organization")]
        public int OrganizationId { get; set; }
        public string? LocationId { get; set; }
        public string? LocationDescription { get; set; }
        [ExcludeFromCodeCoverage]
        public OrganizationModel Organization { get; set; }
        [ExcludeFromCodeCoverage]
        public ICollection<AgencyModel> Agencies { get; set; }
        public bool HasDeduction {  get; set; } =false;
    }
}
