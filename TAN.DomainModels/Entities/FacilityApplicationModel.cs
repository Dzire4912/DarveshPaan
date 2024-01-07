using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Entities
{
    public class FacilityApplicationModel
    {
        [Key]
        public int Id { get; set; }
        public int FacilityId { get; set; }
        public int ApplicationId { get; set; }
        [ExcludeFromCodeCoverage]
        public DateTime CreatedDate { get; set; }
        [ExcludeFromCodeCoverage]
        public string? CreatedBy { get; set; }
        [ExcludeFromCodeCoverage]
        public DateTime UpdatedDate { get; set; }
        [ExcludeFromCodeCoverage]
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
    }
}
