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
    public class AgencyModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? AgencyId { get; set; }
        [Required]
        public string? AgencyName { get; set; }
        [Required]
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        [Required]
        public string? City { get; set; }
        [Required]
        public string? State { get; set; }
        [Required]
        public int ZipCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        [ForeignKey("FacilityModel")]
        public int FacilityId { get; set; }
        [ExcludeFromCodeCoverage]
        public FacilityModel Facility { get; set; }

    }
}
