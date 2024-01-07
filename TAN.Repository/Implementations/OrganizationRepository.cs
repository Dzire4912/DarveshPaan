using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Serilog;
using TAN.ApplicationCore;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Models;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using static System.Reflection.Metadata.BlobBuilder;
using Microsoft.AspNetCore.Identity;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static TAN.DomainModels.Helpers.Permissions;

namespace TAN.Repository.Implementations
{
    [ExcludeFromCodeCoverage]
    public class OrganizationRepository : Repository<OrganizationModel>, IOrganizationRepository
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly UserManager<AspNetUser> _userManager;


        private DatabaseContext context
        {
            get
            {
                return db as DatabaseContext;
            }
        }
        public OrganizationRepository(DbContext db, IHttpContextAccessor httpContext, UserManager<AspNetUser> userManager)
        {
            _httpContext = httpContext;
            this.db = db;
            _userManager = userManager;
        }

        public async Task<string> AddOrganization(OrganizationViewModel model)
        {
            var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var newOrganization = new OrganizationModel()
                {
                    OrganizationName = model.OrganizationName,
                    OrganizationEmail = model.OrganizationEmail,
                    PrimaryPhone = model.PrimaryPhone,
                    SecondaryPhone = model.SecondaryPhone,
                    Country = model.Country,
                    Address1 = model.Address1,
                    Address2 = model.Address2,
                    State = model.State,
                    City = model.City,
                    ZipCode = model.ZipCode,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId,
                    IsActive = true,
                };

                context.Organizations.Add(newOrganization);
                await context.SaveChangesAsync();

                foreach (var serviceType in model.ServiceType)
                {
                    if (serviceType != null && serviceType == Convert.ToInt32(ServiceType.Voice).ToString())
                    {
                        var organizationServices = new OrganizationServices()
                        {
                            OrganizationId = newOrganization.OrganizationID,
                            ServiceType = Convert.ToInt32(serviceType),
                            MasterAccountId = model.MasterAccountId,
                            SubAccountId = model.SubAccountId,
                            CreatedBy = userId,
                            CreatedDate = DateTime.UtcNow,
                            IsActive = true
                        };
                        context.OrganizationServices.Add(organizationServices);
                    }
                    if (serviceType != null && serviceType == Convert.ToInt32(ServiceType.Data).ToString())
                    {
                        var organizationServices = new OrganizationServices()
                        {
                            OrganizationId = newOrganization.OrganizationID,
                            ServiceType = Convert.ToInt32(serviceType),
                            MasterAccountId = null,
                            SubAccountId = null,
                            CreatedBy = userId,
                            CreatedDate = DateTime.UtcNow,
                            IsActive = true
                        };
                        context.OrganizationServices.Add(organizationServices);
                    }
                    await context.SaveChangesAsync();
                }
                return "success";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Organization Controller / Add New Organization  :{ErrorMsg}", ex.Message);
                return "failed";
            }

        }

        public IEnumerable<OrganizationViewModel> GetAllOrganizations(int skip, int take, string? searchValue, string sortOrder, string serviceType, out int organizationCount)
        {
            try
            {
                var lowercaseSearchValue = searchValue?.ToLower();
                var organizations = (from o in context.Organizations
                                     join s in context.States on o.State equals s.StateCode
                                     where o.IsActive &&
                                     (string.IsNullOrEmpty(lowercaseSearchValue) ||
                                         o.OrganizationName.ToLower().Contains(lowercaseSearchValue) ||
                                         o.OrganizationEmail.ToLower().Contains(lowercaseSearchValue) ||
                                         o.PrimaryPhone.ToLower().Contains(lowercaseSearchValue) ||
                                         s.StateName.ToLower().Contains(lowercaseSearchValue))
                                     orderby o.OrganizationID
                                     select new OrganizationViewModel
                                     {
                                         OrganizationID = o.OrganizationID,
                                         OrganizationName = o.OrganizationName,
                                         OrganizationEmail = o.OrganizationEmail,
                                         PrimaryPhone = o.PrimaryPhone,
                                         State = s.StateName,
                                         ServiceTypes = (
                             from os in context.OrganizationServices
                             where os.OrganizationId == o.OrganizationID && os.IsActive
                             select os.ServiceType == Convert.ToInt32(ServiceType.Voice) ? "Voice" : (os.ServiceType == Convert.ToInt32(ServiceType.Data) ? "Data" : "")
                         ).Any() ? string.Join(", ",
                             from os in context.OrganizationServices
                             where os.OrganizationId == o.OrganizationID && os.IsActive
                             select os.ServiceType == Convert.ToInt32(ServiceType.Voice) ? "Voice" : (os.ServiceType == Convert.ToInt32(ServiceType.Data) ? "Data" : "")
                         ) : "NA"
                                     }).AsEnumerable();

                if (!string.IsNullOrEmpty(serviceType))
                {
                    organizations = organizations.Where(o => o.ServiceTypes.Contains(serviceType)).AsEnumerable();
                }

                organizationCount = organizations.Count();

                organizations = organizations.Skip(skip).Take(take);




                //sort data
                if (!string.IsNullOrEmpty(sortOrder))
                {
                    switch (sortOrder)
                    {
                        case "organizationname_asc":
                            organizations = organizations.OrderBy(x => x.OrganizationName);
                            break;
                        case "organizationname_desc":
                            organizations = organizations.OrderByDescending(x => x.OrganizationName);
                            break;
                        case "organizationemail_asc":
                            organizations = organizations.OrderBy(x => x.OrganizationEmail);
                            break;
                        case "organizationemail_desc":
                            organizations = organizations.OrderByDescending(x => x.OrganizationEmail);
                            break;
                        case "primaryphone_asc":
                            organizations = organizations.OrderBy(x => x.PrimaryPhone);
                            break;
                        case "primaryphone_desc":
                            organizations = organizations.OrderByDescending(x => x.PrimaryPhone);
                            break;
                        case "state_asc":
                            organizations = organizations.OrderBy(x => x.State);
                            break;
                        case "state_desc":
                            organizations = organizations.OrderByDescending(x => x.State);
                            break;
                    }
                }
                return organizations;
            }
            catch (Exception ex)
            {
                organizationCount = 0;
                Log.Error(ex, "An Error Occured in Organization Controller / Viewing All Organizations :{ErrorMsg}", ex.Message);
                return Enumerable.Empty<OrganizationViewModel>();
            }
        }
        public async Task<OrganizationViewModel> GetOrganizationDetail(int organizationId)
        {
            try
            {
                var organization = await context.Organizations.Where(o => o.OrganizationID == organizationId).FirstOrDefaultAsync();
                var organizationServices = context.OrganizationServices.Where(o => o.OrganizationId == organizationId).ToList();
                OrganizationViewModel organizationViewModel = new OrganizationViewModel()
                {
                    OrganizationID = organization.OrganizationID,
                    OrganizationName = organization.OrganizationName,
                    OrganizationEmail = organization.OrganizationEmail,
                    Country = organization.Country,
                    PrimaryPhone = organization.PrimaryPhone,
                    SecondaryPhone = organization.SecondaryPhone,
                    Address1 = organization.Address1,
                    Address2 = organization.Address2,
                    State = organization.State,
                    City = organization.City,
                    ZipCode = organization.ZipCode,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    IsActive = organization.IsActive,
                };

                if (organizationServices.Count > 0)
                {
                    organizationViewModel.serviceDetails = new List<ServiceDetails>();
                    organizationViewModel.ServiceType = new List<string>();
                    foreach (var serviceType in organizationServices)
                    {
                        if (serviceType.IsActive)
                        {
                            ServiceDetails serviceDetails = new ServiceDetails();
                            serviceDetails.ServiceType = serviceType.ServiceType.ToString();
                            serviceDetails.MasterAccountId = serviceType.MasterAccountId;
                            serviceDetails.SubAccountId = serviceType.SubAccountId;
                            organizationViewModel.serviceDetails.Add(serviceDetails);
                        }
                    }
                }
                return organizationViewModel;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Organization Controller / Getting Organization Details :{ErrorMsg}", ex.Message);
                return new OrganizationViewModel();
            }
        }

        public async Task<string> UpdateOrganization(OrganizationViewModel organizationViewModel)
        {
            try
            {
                var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                var existingOrganization = await context.Organizations.Where(o => o.OrganizationID == organizationViewModel.OrganizationID).FirstOrDefaultAsync();
                var organizationServices = context.OrganizationServices.Where(o => o.OrganizationId == organizationViewModel.OrganizationID).ToList();

                if (existingOrganization != null)
                {
                    existingOrganization.OrganizationName = organizationViewModel.OrganizationName;
                    existingOrganization.OrganizationEmail = organizationViewModel.OrganizationEmail;
                    existingOrganization.Country = organizationViewModel.Country;
                    existingOrganization.PrimaryPhone = organizationViewModel.PrimaryPhone;
                    existingOrganization.SecondaryPhone = organizationViewModel.SecondaryPhone;
                    existingOrganization.Address1 = organizationViewModel.Address1;
                    existingOrganization.Address2 = organizationViewModel.Address2;
                    existingOrganization.State = organizationViewModel.State;
                    existingOrganization.City = organizationViewModel.City;
                    existingOrganization.ZipCode = organizationViewModel.ZipCode;
                    existingOrganization.UpdatedDate = DateTime.UtcNow;
                    existingOrganization.UpdatedBy = userId;
                    existingOrganization.IsActive = true;
                    context.Organizations.Update(existingOrganization);
                    await context.SaveChangesAsync();

                    if (organizationServices.Count > 0)
                    {
                        foreach (var organizationService in organizationServices)
                        {
                            organizationService.IsActive = false;
                            organizationService.UpdatedDate = DateTime.UtcNow;
                            organizationService.UpdatedBy = userId;
                        }

                        foreach (var serviceType in organizationViewModel.ServiceType)
                        {
                            var existingOrganizationService = organizationServices.FirstOrDefault(os => os.ServiceType.ToString() == serviceType);
                            if (existingOrganizationService != null)
                            {
                                existingOrganizationService.IsActive = true;
                                existingOrganizationService.UpdatedDate = DateTime.UtcNow;
                                existingOrganizationService.UpdatedBy = userId;
                                if (existingOrganizationService.ServiceType == 0)
                                    existingOrganizationService.SubAccountId = organizationViewModel.SubAccountId;
                            }
                            else
                            {
                                if (serviceType != null && serviceType == Convert.ToInt32(ServiceType.Voice).ToString())
                                {

                                    var organizationServicesModel = new OrganizationServices()
                                    {
                                        OrganizationId = existingOrganization.OrganizationID,
                                        ServiceType = Convert.ToInt32(serviceType),
                                        MasterAccountId = organizationViewModel.MasterAccountId,
                                        SubAccountId = organizationViewModel.SubAccountId,
                                        CreatedBy = userId,
                                        CreatedDate = DateTime.UtcNow,
                                        IsActive = true
                                    };
                                    context.OrganizationServices.Add(organizationServicesModel);
                                }
                                if (serviceType != null && serviceType == Convert.ToInt32(ServiceType.Data).ToString())
                                {
                                    var organizationServicesModel = new OrganizationServices()
                                    {
                                        OrganizationId = existingOrganization.OrganizationID,
                                        ServiceType = Convert.ToInt32(serviceType),
                                        MasterAccountId = null,
                                        SubAccountId = null,
                                        CreatedBy = userId,
                                        CreatedDate = DateTime.UtcNow,
                                        IsActive = true
                                    };
                                    context.OrganizationServices.Add(organizationServicesModel);
                                }
                            }
                            await context.SaveChangesAsync();
                        }
                    }
                    else if (organizationServices.Count == 0 && organizationViewModel.ServiceType.Count >= 1)
                    {
                        foreach (var serviceType in organizationViewModel.ServiceType)
                        {
                            if (serviceType != null && serviceType == Convert.ToInt32(ServiceType.Voice).ToString())
                            {
                                var newOrganizationServices = new OrganizationServices()
                                {
                                    OrganizationId = existingOrganization.OrganizationID,
                                    ServiceType = Convert.ToInt32(serviceType),
                                    MasterAccountId = organizationViewModel.MasterAccountId,
                                    SubAccountId = organizationViewModel.SubAccountId,
                                    CreatedBy = userId,
                                    CreatedDate = DateTime.UtcNow,
                                    IsActive = true
                                };
                                context.OrganizationServices.Add(newOrganizationServices);
                            }
                            if (serviceType != null && serviceType == Convert.ToInt32(ServiceType.Data).ToString())
                            {
                                var newOrganizationServices = new OrganizationServices()
                                {
                                    OrganizationId = existingOrganization.OrganizationID,
                                    ServiceType = Convert.ToInt32(serviceType),
                                    MasterAccountId = null,
                                    SubAccountId = null,
                                    CreatedBy = userId,
                                    CreatedDate = DateTime.UtcNow,
                                    IsActive = true
                                };
                                context.OrganizationServices.Add(newOrganizationServices);
                            }
                            await context.SaveChangesAsync();
                        }
                    }
                    return "success";
                }
                else
                {
                    return "failed";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Organization Controller / Updating Existing Organization :{ErrorMsg}", ex.Message);
                return "failed";
            }
        }

        public async Task<string> DeleteOrganizationDetail(int organizationId)
        {
            try
            {
                var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                var organization = context.Organizations.Where(o => o.OrganizationID == organizationId).FirstOrDefault();
                if (organization != null)
                {
                    var currentOrganizationFacilities = context.Facilities.Where(o => o.OrganizationId == organizationId).ToList();
                    var currentOrganizationServices = context.OrganizationServices.Where(o => o.OrganizationId == organizationId).ToList();
                    if (currentOrganizationFacilities != null)
                    {
                        foreach (var orgFacility in currentOrganizationFacilities)
                        {
                            orgFacility.IsActive = false;
                            orgFacility.UpdatedBy = userId;
                            orgFacility.UpdatedDate = DateTime.UtcNow;
                            context.Facilities.Update(orgFacility);

                            var existingOrganizationFacilities = context.OrganizationFacilities.Where(of => of.FacilityId == orgFacility.Id).ToList();
                            var facilityApplication = context.FacilityApplications.Where(fa => fa.FacilityId == orgFacility.Id).ToList();
                            var facilityUsers = await context.UserOrganizationFacilities.Where(uof => uof.FacilityID == orgFacility.Id).ToListAsync();

                            if (existingOrganizationFacilities.Count > 0)
                            {
                                foreach (var existingOrganizationFacility in existingOrganizationFacilities)
                                {
                                    existingOrganizationFacility.IsActive = false;
                                    existingOrganizationFacility.UpdatedDate = DateTime.UtcNow;
                                    existingOrganizationFacility.UpdatedBy = userId;
                                    context.OrganizationFacilities.Update(existingOrganizationFacility);
                                }
                            }
                            if (facilityApplication.Count > 0)
                            {
                                foreach (var application in facilityApplication)
                                {
                                    application.IsActive = false;
                                    application.UpdatedDate = DateTime.UtcNow;
                                    application.UpdatedBy = userId;
                                    context.FacilityApplications.Update(application);
                                }
                            }

                            if (facilityUsers.Count > 0)
                            {
                                foreach (var facilityUser in facilityUsers)
                                {
                                    var userDetails = context.Users.Where(u => u.Id == facilityUser.UserId).FirstOrDefault();
                                    if (userDetails != null)
                                    {
                                        userDetails.IsActive = false;
                                        userDetails.UpdatedDate = DateTime.UtcNow;
                                        userDetails.ModifiedBy = userId;
                                        context.Users.Update(userDetails);
                                    }
                                    facilityUser.IsActive = false;
                                    facilityUser.UpdatedDate = DateTime.UtcNow;
                                    facilityUser.UpdatedBy = userId;
                                    context.UserOrganizationFacilities.Update(facilityUser);

                                    var facilityUserApplications = context.UserApplications.Where(fua => fua.UserId == facilityUser.UserId).ToList();
                                    if (facilityUserApplications.Count > 0)
                                    {
                                        foreach (var facilityUserApplication in facilityUserApplications)
                                        {
                                            context.UserApplications.Remove(facilityUserApplication);
                                        }
                                    }

                                    var userRolesDetails = context.UserRoles.Where(ur => ur.UserId == facilityUser.UserId).ToList();
                                    if (userRolesDetails.Count > 0)
                                    {
                                        foreach (var userRolesDetail in userRolesDetails)
                                        {
                                            context.UserRoles.Remove(userRolesDetail);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (currentOrganizationServices.Count > 0)
                    {
                        foreach (var orgService in currentOrganizationServices)
                        {
                            orgService.IsActive = false;
                            orgService.UpdatedBy = userId;
                            orgService.UpdatedDate = DateTime.UtcNow;
                            context.OrganizationServices.Update(orgService);
                        }
                    }
                    organization.IsActive = false;
                    organization.UpdatedBy = userId;
                    organization.UpdatedDate = DateTime.UtcNow;
                    context.Organizations.Update(organization);
                    await context.SaveChangesAsync();
                    return "success";
                }
                else
                {
                    return "failed";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Organization Controller / Delete Existing Organization :{ErrorMsg}", ex.Message);
                return "failed";
            }
        }

        public async Task<bool> GetEmailId(string emailId, int organizationId)
        {
            var result = await context.Organizations.Where(o => o.OrganizationEmail == emailId).FirstOrDefaultAsync();
            if (result != null)
            {
                if (result.OrganizationID == organizationId)
                    return false;
                else
                    return true;
            }
            else
                return false;
        }
        public async Task<bool> GetOrganizationName(string organizationName, int organizationId)
        {
            try
            {
                var result = await context.Organizations.Where(o => o.OrganizationName == organizationName).FirstOrDefaultAsync();
                if (result != null)
                {
                    if (result.OrganizationID == organizationId)
                        return false;
                    else
                        return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Organization Controller / Getting Organization Name :{ErrorMsg}", ex.Message);
                return false;
            }
        }

        public BandWidthMasterViewModel GetMasterAccountDetails()
        {
            BandWidthMasterViewModel bandWidthMasterViewModel = new BandWidthMasterViewModel();
            try
            {
                var result = context.BandwidthMaster.FirstOrDefault();
                if (result != null)
                {
                    bandWidthMasterViewModel.AccountId = result.AccountId;
                    bandWidthMasterViewModel.AccountName = result.AccountName;

                    return bandWidthMasterViewModel;
                }
                else
                    return new BandWidthMasterViewModel();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Organization Controller / Getting Master Account Details :{ErrorMsg}", ex.Message);
                return new BandWidthMasterViewModel();
            }
        }

        public async Task<List<BandwidthAccessList>> GetBandwidthAccessList()
        {
            List<BandwidthAccessList> bandwidthAccessList = new List<BandwidthAccessList>();
            try
            {
                bandwidthAccessList = await (from org in context.Organizations
                                             join orgs in context.OrganizationServices on org.OrganizationID equals orgs.OrganizationId
                                             where org.IsActive && orgs.IsActive && orgs.ServiceType == (int)ServiceType.Voice
                                             select new BandwidthAccessList
                                             {
                                                 MasterAccountId = orgs.MasterAccountId,
                                                 SubAccountId = orgs.SubAccountId,
                                                 OrganizationName = org.OrganizationName
                                             }).AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Organization Controller / GetBandwidthAccessList :{ErrorMsg}", ex.Message);
            }
            return bandwidthAccessList;
        }

        public async Task<bool> CheckSubAccountId(int organizationId, string subAccountId)
        {
            try
            {
                var result = await context.OrganizationServices.Where(o => o.SubAccountId == subAccountId).FirstOrDefaultAsync();
                if (result != null)
                {
                    if (result.OrganizationId == organizationId)
                        return false;
                    else
                        return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Organization Controller / Checking Sub-Account Id :{ErrorMsg}", ex.Message);
                return false;
            }
        }

        public async Task<List<Itemlist>> GetApplicationWiseOrganizations(string appName)
        {
            List<Itemlist> organizations = new List<Itemlist>();
            List<Itemlist> facilityOrganizations = new List<Itemlist>();
            List<Itemlist> facilities = new List<Itemlist>();
            try
            {
                var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                var appInfo = context.Application.FirstOrDefault(a => a.Name.ToLower() == appName.ToLower());
                if (userId != null)
                {
                    var UserData = await _userManager.FindByIdAsync(userId);
                    if (UserData?.UserType == (int)UserTypes.Thinkanew)
                    {
                        var allServiceOrganizations = context.OrganizationServices.Where(o => o.IsActive).ToList();
                        if (appInfo.ApplicationId == (int)ApplicationNameWithId.PBJSnap)
                        {
                            organizations = context.Organizations.Where(o => o.IsActive).AsEnumerable().Where(o => !allServiceOrganizations.Any(so => so.OrganizationId == o.OrganizationID)).Select(o => new Itemlist { Text = o.OrganizationName, Value = o.OrganizationID }).ToList();
                            facilities = (from f in context.Facilities
                                          join fa in context.FacilityApplications on f.Id equals fa.FacilityId
                                          where fa.ApplicationId == (int)ApplicationNameWithId.PBJSnap
                                          select new Itemlist { Value = f.Id, Text = f.FacilityName }).ToList();
                            foreach (var facility in facilities)
                            {
                              var facilityOrganization = (from o in context.Organizations
                                                                   join f in context.Facilities on o.OrganizationID equals f.OrganizationId where f.Id == facility.Value
                                                                   select new Itemlist { Value = o.OrganizationID, Text = o.OrganizationName }).FirstOrDefault();
                                facilityOrganizations.Add(facilityOrganization);
                            }

                            organizations = organizations.Union(facilityOrganizations).GroupBy(org => org.Value).Select(group => group.First()).ToList();
                        }
                        else
                        {
                            organizations = context.Organizations.Where(o => o.IsActive).AsEnumerable().Where(o => allServiceOrganizations.Any(so => so.OrganizationId == o.OrganizationID)).Select(o => new Itemlist { Text = o.OrganizationName, Value = o.OrganizationID }).ToList();
                            facilities = (from f in context.Facilities
                                          join fa in context.FacilityApplications on f.Id equals fa.FacilityId
                                          where fa.ApplicationId == (int)ApplicationNameWithId.Reporting
                                          select new Itemlist { Value = f.Id, Text = f.FacilityName }).ToList();
                            foreach (var facility in facilities)
                            {
                                var facilityOrganization = (from o in context.Organizations
                                                            join f in context.Facilities on o.OrganizationID equals f.OrganizationId
                                                            where f.Id == facility.Value
                                                            select new Itemlist { Value = o.OrganizationID, Text = o.OrganizationName }).FirstOrDefault();
                                facilityOrganizations.Add(facilityOrganization);
                            }

                            organizations = organizations.Union(facilityOrganizations).GroupBy(org => org.Value).Select(group => group.First()).ToList();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Organization Controller / Get Organizations by Application :{ErrorMsg}", ex.Message);
            }
            return organizations;
        }
    }
}
