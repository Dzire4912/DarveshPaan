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
    public class PayTypeCodes
    {
        public int PayTypeCode { get; set; }
        public string? PayTypeDescription { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? CreateBy { get; set; }
        public string? UpdateBy { get; set; }
        public bool IsActive { get; set; }
    }
}
