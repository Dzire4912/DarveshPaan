using TAN.DomainModels.Entities;
using TAN.DomainModels.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Models;

namespace TAN.Repository.Abstractions
{
    public interface IFacilityRepository : IRepository<FacilityModel>
    {
        Task<string> AddFacility(FacilityViewModel model);

        IEnumerable<FacilityViewModel> GetAllFacilities(int skip, int pageSize, string? searchValue, string sortOrder, string orgName, string appName, string organizationId, out int facilityCount);

        Task<FacilityViewModel> GetFacilityDetail(int facilityId);

        Task<string> UpdateFacility(FacilityViewModel facilityViewModel);

        Task<string> DeleteFacilityDetail(int facilityId);

        List<StateModel> GetStateList();
        Task<List<FacilityModel>> GetAllFacilitiesByOrgId();
        Task<bool> GetFacilityName(string facilityName, string facilityId);
        Task<bool> GetFacilityId(string facilityId);
        bool CheckRole();
        Task<List<FacilityModel>> GetAllFacilitiesByOrgId(int orgId);
        ServiceOrganizationViewModel GetOrganizationServiceDetails(int organizationId);
        string? GetOrganizationUserDetails();
        bool GetLoggedInUserInfo();
        Task<List<FacilityLocationAccess>> GetAllFacilitiesLocationIdByOrgId();
    }
}
