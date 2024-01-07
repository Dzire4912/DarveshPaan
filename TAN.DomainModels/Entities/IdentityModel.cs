using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
	
	public class AspNetRoles : IdentityRole
	{
		[Column(TypeName = "varchar(50)")]
		public string? RoleType { get; set; }
		public string? ApplicationId { get; set; }
	    [SkipAudit]
        public DateTime? CreatedAt { get; set; }=DateTime.UtcNow;
        [SkipAudit]
        [ExcludeFromCodeCoverage]
        public DateTime? UpdatedAt { get; set; }
        [SkipAudit]
        [ExcludeFromCodeCoverage]
        public string? CreatedBy { get; set; }
        [SkipAudit]
        [ExcludeFromCodeCoverage]
        public string? UpdatedBy { get; set; }
        [SkipAudit]
        public bool IsActive { get; set; } = true;
	}

	public class AspNetUserRoles : IdentityUserRole<string>
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
	}
}
