using Microsoft.EntityFrameworkCore;
using TAN.ApplicationCore;
using TAN.DomainModels.Entities;
using TAN.Repository.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace TAN.Repository.Implementations
{
    [ExcludeFromCodeCoverage]
    public class ModulePermissionRepository : Repository<Module>, IModulePermissionRepository
    {
        private DatabaseContext Context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public ModulePermissionRepository(DbContext db)
        {
            this.db = db;

        }


        public IEnumerable<Module> GetMenuMaster()
        {
            return Context.Modules.AsEnumerable();

        }

        public IEnumerable<Module> GetMenuMaster(string UserRole)
        {
            var result = (from module in Context.Modules
                          join mp in Context.ModulePermissions
                          on module.ModuleId equals mp.ModuleId
                          where mp.RoleId == UserRole
                          orderby module.ModuleId ascending
                          select new Module
                          {
                              ModuleId = module.ModuleId,
                              Name = module.Name,
                              IsDisplay = module.IsDisplay,
                              Path = module.Path,
                              icon = module.icon,
                              Parent = module.Parent,
                              CreatedDate = module.CreatedDate,
                              UpdatedDate = module.UpdatedDate,
                              DeletedAt = module.DeletedAt,
                          }).ToList();

            return (IEnumerable<Module>)result;
        }
    }
}
