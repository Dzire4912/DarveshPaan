using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Models;
using TAN.DomainModels.ViewModels;

namespace TAN.Repository.Abstractions
{
    public interface IOrganizationRepository:IRepository<OrganizationModel>
    {
        Task<string> AddOrganization(OrganizationViewModel model);

        IEnumerable<OrganizationViewModel> GetAllOrganizations(int skip, int take, string? searchValue, string sortOrder,string serviceType, out int organizationCount);

        Task<OrganizationViewModel> GetOrganizationDetail(int organizationId);

        Task<string> UpdateOrganization(OrganizationViewModel organizationViewModel);

        Task<string> DeleteOrganizationDetail(int organizationId);

        Task<bool> GetEmailId(string emailId, int organizationId);

        Task<bool> GetOrganizationName(string organizationName, int organizationId);
        BandWidthMasterViewModel GetMasterAccountDetails();
        Task<bool> CheckSubAccountId(int organizationId, string subAccountId);
        Task<List<BandwidthAccessList>> GetBandwidthAccessList();
        Task<List<Itemlist>> GetApplicationWiseOrganizations(string appName);
    }
}
