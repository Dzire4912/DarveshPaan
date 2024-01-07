using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace TAN.DomainModels.Entities
{
    public class OrganizationModel
    {
        [Key]
        public int OrganizationID { get; set; }
        public string? OrganizationName { get; set; }
        public string? OrganizationEmail { get; set; }
        public string? PrimaryPhone { get; set; }
        public string? SecondaryPhone { get; set; }
        public string? Country { get; set; }
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
        [ExcludeFromCodeCoverage]
        public ICollection<FacilityModel> Facilities { get; set; }
    }
}
