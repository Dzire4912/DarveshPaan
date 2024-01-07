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
    public class PermissionRepository : Repository<Permission>, IPermissionsRepository
    {
        private DatabaseContext Context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public PermissionRepository(DbContext db)
        {
            this.db = db;
        }
        public IEnumerable<Permission> GetPermissions(int ModuleId,int appId)
        {
            var result = new List<Permission>();
            if (ModuleId != 0 && appId!=0)
            {
                try
                {
                    result = (from permissions in Context.Permissions
                              join menu in Context.Modules on permissions.ModuleId equals menu.ModuleId
                              where permissions.IsActive  && (permissions.ModuleId == ModuleId || menu.Parent == ModuleId) && (permissions.Modules.ApplicationId == appId)

                              select new Permission
                              {
                                  ModuleId = ModuleId,
                                  Name = permissions.Name
                              }).ToList();
                }
                catch(Exception ex) 
                { 
                    //LOG
                }
            }
            if (ModuleId !=0 && appId == 0)
            {
                try
                {
                    result = (from permissions in Context.Permissions
                              join menu in Context.Modules on permissions.ModuleId equals menu.ModuleId
                              where permissions.IsActive  && (permissions.ModuleId == ModuleId || menu.Parent == ModuleId) 

                              select new Permission
                              {
                                  ModuleId = ModuleId,
                                  Name = permissions.Name
                              }).ToList();
                }
                catch (Exception ex)
                { 
                //LOG
                }
            }
            if (ModuleId == 0 && appId != 0)
            {
                try
                {
                    result = (from permissions in Context.Permissions
                              join menu in Context.Modules on permissions.ModuleId equals menu.ModuleId
                              where permissions.IsActive  && (permissions.Modules.ApplicationId == appId)

                              select new Permission
                              {
                                  ModuleId = ModuleId,
                                  Name = permissions.Name
                              }).ToList();
                }
                catch (Exception ex)
                { 
                //LOG
                }
            }
            return (IEnumerable<Permission>)result;
        }

       
    }
}
