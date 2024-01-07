using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;

namespace TAN.Repository.Abstractions
{
    public interface IAspNetUserRoleRepository: IRepository<AspNetUserRoles>
    {
        Task<int> GetContractorCountByFacility(int OrgId);
    }
}
