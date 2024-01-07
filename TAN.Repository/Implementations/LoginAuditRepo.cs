using Microsoft.EntityFrameworkCore;
using Serilog;
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
    public class LoginAuditRepo : Repository<LoginAudit>, ILoginAudit
    {
        private DatabaseContext Context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public LoginAuditRepo(DbContext db)
        {
            this.db = db;
        }

        public async Task<bool> LoginValidate(string UserEmail)
        {
            try
            {
                var query = from a in Context.AspNetUsers
                            join uof in Context.UserOrganizationFacilities on a.Id equals uof.UserId
                            join o in Context.Organizations on uof.OrganizationID equals o.OrganizationID
                            join f in Context.Facilities on uof.FacilityID equals f.Id
                            join fa in Context.FacilityApplications on uof.FacilityID equals fa.FacilityId
                            join app in Context.Application on fa.ApplicationId equals app.ApplicationId
                            join ar in Context.AspNetUserRoles on a.Id equals ar.UserId
                            join anr in Context.AspNetRoles on ar.RoleId equals anr.Id
                            where a.Email == UserEmail
                               && o.IsActive && f.IsActive && uof.IsActive && fa.IsActive && app.IsActive && a.IsActive
                               && anr.IsActive && app.ApplicationId == a.DefaultAppId
                               && Convert.ToInt32(anr.ApplicationId) == a.DefaultAppId
                            select new
                            {
                                a.Id,
                                a.UserName,
                                o.OrganizationName,
                                f.FacilityName,
                                f.FacilityID,
                                app.Name,
                                app.ApplicationId,
                                RoleName = anr.Name,
                                a.IsActive
                            };
                 
                if (query.Any())
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured LoginValidate :{ErrorMsg}", ex.Message);
            }
            return false;
        }
    }
}
