using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Helpers;

namespace TAN.DomainModels.Entities
{
    [SkipAudit]
    [ExcludeFromCodeCoverage]
    public class LoginAudit
    {
        [Key]
        public int LoginAuditId { get; set; }
        public string? IPAddress { get; set; }        
        public string UserId { get; set;}
        [ForeignKey("UserId")]
        public AspNetUser users { get; set; }
        public int LoginStatus { get; set;}       
        public DateTime LoginTime { get; set;}
        public DateTime? LogOutTime { get; set; }
        public string? UserAgent { get; set; }
    }
    
}
