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
    public class ModulePermission
    {
        public ModulePermission()
        {
            CreatedAt = DateTime.Now;
        }
        [Key]
        public int PermissionId { get; set; }
        public string RoleId { get; set; }
        public Module Modules { get; set; }
        [ForeignKey("Module")]
        public int ModuleId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
    }
}
