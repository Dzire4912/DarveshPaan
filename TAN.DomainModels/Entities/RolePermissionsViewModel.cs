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
    public class RolePermissionsViewModel
    {
        [Key]
        public int Id { get; set; }
        public string RoleName { get; set; }
        public string RoleId { get; set; }
        public string Permissions { get; set; }
    }
}
