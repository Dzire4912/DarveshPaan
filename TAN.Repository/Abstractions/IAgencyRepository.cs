using TAN.DomainModels.Entities;
using TAN.DomainModels.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.Repository.Abstractions
{
    public interface IAgencyRepository : IRepository<AgencyModel>
    {
        Task<string> AddAgency(AgencyViewModel model);
        IEnumerable<AgencyViewModel> GetAllAgenciesList(int skip, int pageSize, string? searchValue, string sortOrder, string facilityName, string agencyId, string organizationId, out int agencyCount);
        Task<AgencyViewModel> GetAgencyDetails(string agencyId);
        Task<string> UpdateAgency(AgencyViewModel agencyViewModel);
        Task<string> DeleteAgencyDetail(int agencyId);
        Task<List<AgencyModel>> GetAllAgeniciesByFacilityId(int facilityId);
        Task<bool> GetAgencyName(string agencyName, string agencyId);
        Task<bool> GetAgencyId(string agencyId);
        Task<int> GetFacilityAgencyCount(int OrgId);

    }
}
