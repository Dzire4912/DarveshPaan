using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TAN.DomainModels.Helpers;
using System.Diagnostics.CodeAnalysis;

namespace TAN.DomainModels.Entities
{
    [SkipAudit]
    public class AspNetUser: IdentityUser
    {
        public int UserType { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int IsUsingTempPassword { get; set; } = 0;
        public bool IsActive { get; set; } 
        public string? CreatedBy { get; set; }
        [ExcludeFromCodeCoverage]
        public string? ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        [ExcludeFromCodeCoverage]
        public DateTime? UpdatedDate { get; set; }
        public int DefaultAppId { get; set; }
    }

    public class UserApplication
    {
        [Key]
        public int Id { get; set; }
        [ExcludeFromCodeCoverage]
        public AspNetUser AspNetUser { get; set; }
        [ForeignKey("AspNetUser")]
        public string UserId { get; set; }
        [ExcludeFromCodeCoverage]
        public Application Application { get; set; }
        [ForeignKey("Application")]
        public int ApplicationId { get; set; }
    }
}
