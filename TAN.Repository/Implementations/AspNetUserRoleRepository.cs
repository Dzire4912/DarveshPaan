using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.ApplicationCore;
using TAN.DomainModels.Entities;
using TAN.Repository.Abstractions;

namespace TAN.Repository.Implementations
{
    [ExcludeFromCodeCoverage]
    public class AspNetUserRoleRepository : Repository<AspNetUserRoles>, IAspNetUserRoleRepository
    {
        private DatabaseContext context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public AspNetUserRoleRepository(DbContext db)
        {
            this.db = db;
        }

        public async Task<int> GetContractorCountByFacility(int OrgId)
        {
            int count = 0;
            if (OrgId > 0)
            {
                var Contractors = from r in context.AspNetUserRoles
                        join ro in context.Roles on r.RoleId equals ro.Id
                        join u in context.Users on r.UserId equals u.Id
                        join uf in context.UserOrganizationFacilities on r.UserId equals uf.UserId
                        join o in context.Organizations on uf.OrganizationID equals o.OrganizationID
                        join f in context.Facilities on uf.FacilityID equals f.Id
                        where f.IsActive && uf.OrganizationID == OrgId && ro.NormalizedName == "CONTRACTOR" && u.IsActive && uf.IsActive && o.IsActive && uf.IsActive
                        select r;
                count = Contractors != null ? Contractors.Count() : 0;
            }
            else
            {
                var Contractors = from r in context.AspNetUserRoles
                                  join ro in context.Roles on r.RoleId equals ro.Id
                                  join u in context.Users on r.UserId equals u.Id
                                  join uf in context.UserOrganizationFacilities on r.UserId equals uf.UserId
                                  join o in context.Organizations on uf.OrganizationID equals o.OrganizationID
                                  join f in context.Facilities on uf.FacilityID equals f.Id
                                  where f.IsActive && ro.NormalizedName == "CONTRACTOR" && u.IsActive && uf.IsActive && o.IsActive && uf.IsActive
                                  select r;
                count = Contractors != null ? Contractors.Count() : 0;
            }
            return count;
        }

    }
}
