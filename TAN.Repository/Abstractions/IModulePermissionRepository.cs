using TAN.DomainModels.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.Repository.Abstractions
{
    public interface IModulePermissionRepository : IRepository<Module>
    {
        IEnumerable<Module> GetMenuMaster();
        IEnumerable<Module> GetMenuMaster(String UserRole);
    }
}
