using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using TAN.ApplicationCore;
using TAN.DomainModels.Entities;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Net.Cache;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using static TAN.DomainModels.Helpers.Permissions;
using System.Diagnostics.Contracts;
using TAN.ApplicationCore.Migrations;

namespace TAN.Repository.Implementations
{
    [ExcludeFromCodeCoverage]
    public class AgencyRepository : Repository<AgencyModel>, IAgencyRepository
    {
        private readonly IHttpContextAccessor _httpContext;

        private DatabaseContext context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public AgencyRepository(DbContext db, IHttpContextAccessor httpContext)
        {
            this.db = db;
            _httpContext = httpContext;
        }

        public async Task<string> AddAgency(AgencyViewModel model)
        {
            try
            {
                var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                var agency = context.Agencies.Where(a => a.AgencyId == model.AgencyId).FirstOrDefault();

                if (agency == null)
                {
                    var newAgency = new AgencyModel();

                    foreach (var facilityId in model.FacilityId)
                    {
                        if (facilityId != null)
                        {
                            newAgency = new AgencyModel()
                            {
                                AgencyId = model.AgencyId,
                                AgencyName = model.AgencyName,
                                Address1 = model.Address1,
                                Address2 = model.Address2,
                                State = model.State,
                                City = model.City,
                                ZipCode = Convert.ToInt32(model.ZipCode),
                                CreatedDate = DateTime.UtcNow,
                                CreatedBy = userId,
                                IsActive = true,
                                FacilityId = Convert.ToInt32(facilityId)
                            };

                            context.Agencies.Add(newAgency);
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
                Log.Error(ex, "An Error Occured  in Agency Controller / Add New Agency :{ErrorMsg}", ex.Message);
                return "failed";
            }
        }

        public IEnumerable<AgencyViewModel> GetAllAgenciesList(int skip, int pageSize, string? searchValue, string sortOrder, string facilityName, string agencyId, string organizationId, out int agencyCount)
        {
            try
            {
                var lowercaseSearchValue = "";
                if (!string.IsNullOrEmpty(searchValue))
                {
                    lowercaseSearchValue = searchValue?.ToLower();
                }

                var agencies = (from a in context.Agencies
                                join f in context.Facilities on a.FacilityId equals f.Id
                                join s in context.States on a.State equals s.StateCode
                                where a.IsActive &&
                                    (string.IsNullOrEmpty(lowercaseSearchValue) ||
                                    a.AgencyId.ToLower().Contains(lowercaseSearchValue) ||
                                    a.AgencyName.ToLower().Contains(lowercaseSearchValue) ||
                                    f.FacilityName.ToLower().Contains(lowercaseSearchValue) ||
                                    s.StateName.ToLower().Contains(lowercaseSearchValue))
                                orderby a.AgencyId
                                select new AgencyViewModel
                                {
                                    Id = a.Id,
                                    AgencyId = a.AgencyId,
                                    AgencyName = a.AgencyName,
                                    FId = f.Id,
                                    FacilityName = f.FacilityName,
                                    State = s.StateName,
                                }).AsEnumerable();

                if (!string.IsNullOrEmpty(facilityName))
                {
                    agencies = agencies.Where(a => a.FacilityName == facilityName).AsEnumerable();
                }

                if (!string.IsNullOrEmpty(agencyId))
                {
                    agencies = agencies.Where(a => a.AgencyId == agencyId).AsEnumerable();
                }

                if (!string.IsNullOrEmpty(organizationId))
                {
                    var organizationFacilities = context.Facilities.Where(f => f.OrganizationId == Convert.ToInt32(organizationId)).ToList();
                    agencies = agencies.Where(a => organizationFacilities.Any(of => of.Id == a.FId)).AsEnumerable();
                }

                agencyCount = agencies.Count();

                agencies = agencies.Skip(skip).Take(pageSize);

                //sort data
                if (!string.IsNullOrEmpty(sortOrder))
                {

                    switch (sortOrder)
                    {
                        case "agencyid_asc":
                            agencies = agencies.OrderBy(x => x.AgencyId);
                            break;
                        case "agencyid_desc":
                            agencies = agencies.OrderByDescending(x => x.AgencyId);
                            break;
                        case "agencyname_asc":
                            agencies = agencies.OrderBy(x => x.AgencyName);
                            break;
                        case "agencyname_desc":
                            agencies = agencies.OrderByDescending(x => x.AgencyName);
                            break;
                        case "facilityname_asc":
                            agencies = agencies.OrderBy(x => x.FacilityName);
                            break;
                        case "facilityname_desc":
                            agencies = agencies.OrderByDescending(x => x.FacilityName);
                            break;
                        case "state_asc":
                            agencies = agencies.OrderBy(x => x.State);
                            break;
                        case "state_desc":
                            agencies = agencies.OrderByDescending(x => x.State);
                            break;
                    }
                }

                return agencies;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Agency Controller / Getting All Agencies :{ErrorMsg}", ex.Message);
                agencyCount = 0;
                return Enumerable.Empty<AgencyViewModel>();
            }
        }

        public async Task<AgencyViewModel> GetAgencyDetails(string agencyId)
        {
            try
            {
                var agency = await context.Agencies.Where(a => a.AgencyId == agencyId).FirstOrDefaultAsync();
                var agencyFacilities = context.Agencies.Where(a => a.AgencyId == agencyId && a.IsActive).ToList();
                AgencyViewModel agencyViewModel = new AgencyViewModel();
                agencyViewModel.FacilityId = new List<string>();
                if (agency != null)
                {
                    agencyViewModel.Id = agency.Id;
                    agencyViewModel.AgencyId = agency.AgencyId.ToString();
                    agencyViewModel.AgencyName = agency.AgencyName;
                    agencyViewModel.Address1 = agency.Address1;
                    agencyViewModel.Address2 = agency.Address2;
                    agencyViewModel.State = agency.State;
                    agencyViewModel.City = agency.City;
                    agencyViewModel.ZipCode = agency.ZipCode.ToString();
                    agencyViewModel.CreatedDate = DateTime.UtcNow;
                    agencyViewModel.UpdateDate = DateTime.UtcNow;
                    agencyViewModel.IsActive = agency.IsActive;
                    foreach (var item in agencyFacilities)
                    {
                        agencyViewModel.FacilityId.Add(item.FacilityId.ToString());
                    }

                    agencyViewModel.FacilityName = agency.FacilityId.ToString();
                }
                return agencyViewModel;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Agency Controller / Getting Agency Detail :{ErrorMsg}", ex.Message);
                return new AgencyViewModel();
            }
        }

        public async Task<string> UpdateAgency(AgencyViewModel agencyViewModel)
        {
            try
            {
                var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                var existingAgency = context.Agencies.Where(a => a.Id == agencyViewModel.Id).FirstOrDefault();

                if (existingAgency != null)
                {
                    var agencyFacilities = context.Agencies.Where(a => a.AgencyId == existingAgency.AgencyId).ToList();
                    foreach (var agencyFacility in agencyFacilities)
                    {
                        agencyFacility.IsActive = false;
                        agencyFacility.UpdatedBy = userId;
                        agencyFacility.UpdatedDate = DateTime.UtcNow;

                        context.Agencies.Update(agencyFacility);
                        await context.SaveChangesAsync();
                    }

                    foreach (var item in agencyViewModel.FacilityId)
                    {
                        if (item == existingAgency.FacilityId.ToString())
                        {
                            existingAgency.AgencyName = agencyViewModel.AgencyName;
                            existingAgency.Address1 = agencyViewModel.Address1;
                            existingAgency.Address2 = agencyViewModel.Address2;
                            existingAgency.State = agencyViewModel.State;
                            existingAgency.City = agencyViewModel.City;
                            existingAgency.ZipCode = Convert.ToInt32(agencyViewModel.ZipCode);
                            existingAgency.UpdatedDate = DateTime.UtcNow;
                            existingAgency.UpdatedBy = userId;
                            existingAgency.IsActive = true;
                            existingAgency.FacilityId = Convert.ToInt32(item);

                            context.Agencies.Update(existingAgency);
                            await context.SaveChangesAsync();

                        }
                        else if (agencyFacilities.Any(a => a.FacilityId.ToString() == item))
                        {
                            var oldAgency = agencyFacilities.Where(a => a.FacilityId.ToString() == item).FirstOrDefault();
                            oldAgency.AgencyName = agencyViewModel.AgencyName;
                            oldAgency.Address1 = agencyViewModel.Address1;
                            oldAgency.Address2 = agencyViewModel.Address2;
                            oldAgency.State = agencyViewModel.State;
                            oldAgency.City = agencyViewModel.City;
                            oldAgency.ZipCode = Convert.ToInt32(agencyViewModel.ZipCode);
                            oldAgency.UpdatedDate = DateTime.UtcNow;
                            oldAgency.UpdatedBy = userId;
                            oldAgency.IsActive = true;
                            oldAgency.FacilityId = Convert.ToInt32(item);

                            context.Agencies.Update(oldAgency);
                            await context.SaveChangesAsync();
                        }
                        else if (item != existingAgency.FacilityId.ToString() && !agencyFacilities.Any(a => a.FacilityId.ToString() == item))
                        {
                            var newAgency = new AgencyModel();
                            newAgency.AgencyId = agencyViewModel.AgencyId;
                            newAgency.AgencyName = agencyViewModel.AgencyName;
                            newAgency.Address1 = agencyViewModel.Address1;
                            newAgency.Address2 = agencyViewModel.Address2;
                            newAgency.State = agencyViewModel.State;
                            newAgency.City = agencyViewModel.City;
                            newAgency.ZipCode = Convert.ToInt32(agencyViewModel.ZipCode);
                            newAgency.CreatedDate = DateTime.UtcNow;
                            newAgency.CreatedBy = userId;
                            newAgency.IsActive = true;
                            newAgency.FacilityId = Convert.ToInt32(item);

                            context.Agencies.Add(newAgency);
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
                Log.Error(ex, "An Error Occured in Agency Controller / Update Existing Agency :{ErrorMsg}", ex.Message);
                return "failed";
            }
        }

        public async Task<string> DeleteAgencyDetail(int agencyId)
        {
            try
            {
                var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                var agency = context.Agencies.Where(a => a.Id == agencyId).FirstOrDefault();
                if (agency != null)
                {
                    agency.IsActive = false;
                    agency.UpdatedBy = userId;
                    agency.UpdatedDate = DateTime.UtcNow;
                    context.Agencies.Update(agency);
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
                Log.Error(ex, "An Error Occured in Agency Controller / Delete Existing Agency :{ErrorMsg}", ex.Message);
                return "failed";
            }
        }

        public async Task<List<AgencyModel>> GetAllAgeniciesByFacilityId(int facilityId)
        {
            List<AgencyModel> agencies = null;
            try
            {
                agencies = await context.Agencies.Where(f => f.FacilityId == facilityId && f.IsActive == true).ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error("GetAllAgeniciesByFacilityId ", ex.Message);
                throw;
            }
            return agencies;
        }

        public async Task<bool> GetAgencyName(string agencyName, string agencyId)
        {
            try
            {
                var result = await context.Agencies.Where(a => a.AgencyName == agencyName).FirstOrDefaultAsync();
                if (result != null)
                {
                    if (result.AgencyId == agencyId)
                        return false;
                    else
                        return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Agency Controller / Getting Agency Name :{ErrorMsg}", ex.Message);
                return false;
            }
        }

        public async Task<bool> GetAgencyId(string agencyId)
        {
            try
            {
                var result = await context.Agencies.Where(a => a.AgencyId == agencyId).FirstOrDefaultAsync();
                if (result != null)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Agency Controller / Getting Agency Id :{ErrorMsg}", ex.Message);
                return false;
            }
        }

        public async Task<int> GetFacilityAgencyCount(int OrgId)
        {
            int count = 0;
            var agency = from a in context.Agencies
                         join f in context.Facilities on a.FacilityId equals f.Id
                         where f.OrganizationId == OrgId && f.IsActive && a.IsActive
                         select a;
            count = agency != null ? agency.Count() : 0;
            return count;
        }


    }
}
