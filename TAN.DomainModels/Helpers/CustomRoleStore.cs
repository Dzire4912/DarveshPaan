using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;

namespace TAN.DomainModels.Helpers
{
    [ExcludeFromCodeCoverage]
    public class CustomRoleStore : RoleStore<AspNetRoles>
    {
        public CustomRoleStore(DbContext context) : base(context)
        {
        }
    }
}
