using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace TAN.DomainModels.Entities
{
    public class UploadHistory
    {
        [Key]
        [ExcludeFromCodeCoverage]
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public int FacilityId { get; set; }
        [ExcludeFromCodeCoverage]
        public DateTime UploadDate { get; set; }
        public string FileName { get; set; }

        [Column(TypeName ="xml")]
        public string UploadXMLFile { get; set; }
        [ExcludeFromCodeCoverage]
        public string? CreatedBy  { get; set; }
        [ExcludeFromCodeCoverage]
        public string? UpdatedBy  { get; set; }
        [ExcludeFromCodeCoverage]
        public DateTime CreatedDate  { get; set; }
        [ExcludeFromCodeCoverage]
        public DateTime UpdatedDate  { get; set; }


        public bool IsActive { get;set; }
        
    }
}
