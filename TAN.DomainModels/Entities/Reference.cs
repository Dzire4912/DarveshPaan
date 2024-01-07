using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Entities
{
    [ExcludeFromCodeCoverage]
    public class Reference
    {
        [Key]
        public int Id { get; set; }
        public int RefereceId { get; set; }
        public string? GroupiTitle { get; set; }
        public string? Groupname { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? DisplayOrder { get; set; }
        public bool Isdeleted { get; set; }=false;


    }
}
