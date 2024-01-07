using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
    [ExcludeFromCodeCoverage]
    public class Permission
    {
        public Permission()
        {
            CreatedAt = DateTime.Now;
        }
        [Key]
        public int Id { get; set; }
        public Module Modules { get; set; }
        [ForeignKey("Module")]
        public int ModuleId { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; }   
    }

    [ExcludeFromCodeCoverage]
    public class RolePermission
    {
        public int Id { get; set; }
        public virtual AspNetRoles AspNetRoles { get; set; }
        public virtual Permission Permission { get; set; }
    }
}
