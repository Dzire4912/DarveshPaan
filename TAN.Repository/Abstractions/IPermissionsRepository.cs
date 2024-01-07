using TAN.DomainModels.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.Repository.Abstractions
{
    public interface IPermissionsRepository : IRepository<Permission>
    {
        IEnumerable<Permission> GetPermissions(int ModuleId,int appId);
    }
}
