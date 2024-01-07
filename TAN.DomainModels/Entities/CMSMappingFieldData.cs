using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Entities
{
    public class CMSMappingFieldData
    {
        public CMSMappingFieldData() { }
        [Key]
        public int Id { get; set; }
        public string FieldName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        [ExcludeFromCodeCoverage]
        public string CreatedBy { get; set; }
        [ExcludeFromCodeCoverage]
        public string UpdatedBy { get; set; }
        public bool IsActive { get; set; }
    }
}
