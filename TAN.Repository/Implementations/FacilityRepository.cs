using Microsoft.EntityFrameworkCore;
using TAN.ApplicationCore;
using TAN.DomainModels.Entities;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Security.Cryptography.X509Certificates;
using TAN.DomainModels.Helpers;
using System.Diagnostics.CodeAnalysis;
using TAN.DomainModels.Models;
using System.Linq;

namespace TAN.Repository.Implementations
{
    [ExcludeFromCodeCoverage]
    public class FacilityRepository : Repository<FacilityModel>, IFacilityRepository
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly Microsoft.AspNetCore.Identity.UserManager<AspNetUser> _userManager;
        private DatabaseContext context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public FacilityRepository(DbContext db, IHttpContextAccessor httpContext, UserManager<AspNetUser> userManager)
        {
            this.db = db;
            _httpContext = httpContext;
            _userManager = userManager;
        }

        public async Task<string> AddFacility(FacilityViewModel model)
        {
            try
            {
                var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                var facility = await context.Facilities.Where(f => f.FacilityID == model.FacilityID).FirstOrDefaultAsync();

                if (facility == null)
                {
                    var newFacility = new FacilityModel()
                    {
                        FacilityID = model.FacilityID,
                        FacilityName = model.FacilityName,
                        OrganizationId = Convert.ToInt32(model.OrganizationId),
                        Address1 = model.Address1,
                        Address2 = model.Address2,
                        State = model.State,
                        City = model.City,
                        ZipCode = model.ZipCode,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = userId,
                        IsActive = true,
                        HasDeduction=model.HasDeduction,
                        LocationId = model.LocationId,
                        LocationDescription = model.LocationDescription
                    };
                    context.Facilities.Add(newFacility);
                    await context.SaveChangesAsync();

                    var newOrganizationFacility = new OrganizationFacilityModel()
                    {
                        OrganizationId = Convert.ToInt32(model.OrganizationId),
                        FacilityId = newFacility.Id,
                        CreatedBy = userId,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow,
                    };

                    context.OrganizationFacilities.Add(newOrganizationFacility);
                    await context.SaveChangesAsync();

                    foreach (var applicationId in model.ApplicationId)
                    {
                        if (applicationId != null)
                        {
                            var newFacilityApplication = new FacilityApplicationModel()
                            {
                                FacilityId = newFacility.Id,
                                ApplicationId = Convert.ToInt32(applicationId),
                                CreatedBy = userId,
                                CreatedDate = DateTime.UtcNow,
                                IsActive = true
                            };

                            context.FacilityApplications.Add(newFacilityApplication);
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
                Log.Error(ex, "An Error Occured in Facility Controller / Add New Facility :{ErrorMsg}", ex.Message);
                return "failed";
            }
        }

        public IEnumerable<FacilityViewModel> GetAllFacilities(int skip, int pageSize, string? searchValue, string sortOrder, string orgName, string appName, string organizationId, out int facilityCount)
        {
            try
            {
                var lowercaseSearchValue = searchValue?.ToLower();

                var allFacilities = context.Facilities
                    .Where(f => f.IsActive)
                    .Join(context.Organizations, f => f.OrganizationId, o => o.OrganizationID, (f, o) => new { f, o })
                    .Join(context.States, fo => fo.f.State, s => s.StateCode, (fo, s) => new { fo.f, fo.o, s })
                    .Join(context.FacilityApplications, fos => fos.f.Id, fa => fa.FacilityId, (fos, fa) => new { fos.f, fos.o, fos.s, fa })
                    .Join(context.Application, fa => fa.fa.ApplicationId, a => a.ApplicationId, (fa, a) => new { fa.f, fa.o, fa.s, fa.fa, a })
                    .Where(fa => fa.f.IsActive && fa.o.IsActive && fa.fa.IsActive)
                    .Where(fa => string.IsNullOrEmpty(lowercaseSearchValue) ||
                        fa.f.FacilityID.Contains(lowercaseSearchValue) ||
                        fa.f.FacilityName.ToLower().Contains(lowercaseSearchValue) ||
                        fa.o.OrganizationName.ToLower().Contains(lowercaseSearchValue) ||
                        fa.s.StateName.ToLower().Contains(lowercaseSearchValue) ||
                        fa.a.Name.ToLower().Contains(lowercaseSearchValue) ||
                        fa.f.LocationId.ToLower().Contains(lowercaseSearchValue))
                    .OrderBy(fa => fa.f.Id)
                    .GroupBy(fa => new { fa.f.Id, fa.f.FacilityID, fa.f.FacilityName, fa.s.StateName, fa.f.LocationId, fa.f.LocationDescription, fa.o.OrganizationName, OrganizationId = fa.f.OrganizationId.ToString() })
                    .Select(g => new FacilityViewModel
                    {
                        Id = g.Key.Id,
                        FacilityID = g.Key.FacilityID,
                        FacilityName = g.Key.FacilityName,
                        State = g.Key.StateName,
                        OrganizationName = g.Key.OrganizationName,
                        OrganizationId = g.Key.OrganizationId,
                        AppId = string.Join(", ", g.Select(x => x.a.Name)),
                        LocationId = g.Key.LocationId == null ? "NA" : g.Key.LocationId,
                        LocationDescription = g.Key.LocationDescription
                    })
                    .AsEnumerable();

                var facilities = allFacilities.Select(f => new FacilityViewModel
                {
                    Id = f.Id,
                    FacilityID = f.FacilityID,
                    FacilityName = f.FacilityName,
                    State = f.State,
                    OrganizationName = f.OrganizationName,
                    OrganizationId = f.OrganizationId,
                    AppId = f.AppId,
                    LocationId = f.LocationId,
                    LocationDescription = f.LocationDescription
                }).AsEnumerable();

                if (!string.IsNullOrEmpty(orgName))
                {
                    facilities = facilities.Where(f => f.OrganizationName == orgName).AsEnumerable();
                }

                if (!string.IsNullOrEmpty(appName))
                {
                    facilities = facilities.Where(f => f.AppId.Contains(appName)).AsEnumerable();
                }

                if (!string.IsNullOrEmpty(organizationId))
                {
                    facilities = facilities.Where(f => f.OrganizationId == organizationId).AsEnumerable();
                }

                facilityCount = facilities.Count();

                facilities = facilities.Skip(skip).Take(pageSize);

                //sort data
                if (!string.IsNullOrEmpty(sortOrder))
                {

                    switch (sortOrder)
                    {
                        case "facilityid_asc":
                            facilities = facilities.OrderBy(x => x.FacilityID);
                            break;

                        case "facilityid_desc":
                            facilities = facilities.OrderByDescending(x => x.FacilityID);
                            break;

                        case "facilityname_asc":
                            facilities = facilities.OrderBy(x => x.FacilityName);
                            break;
                        case "facilityname_desc":
                            facilities = facilities.OrderByDescending(x => x.FacilityName);
                            break;

                        case "organizationname_asc":
                            facilities = facilities.OrderBy(x => x.OrganizationName);
                            break;

                        case "organizationname_desc":
                            facilities = facilities.OrderByDescending(x => x.OrganizationName);
                            break;

                        case "applicationname_asc":
                            facilities = facilities.OrderBy(x => x.AppId);
                            break;

                        case "applicationname_desc":
                            facilities = facilities.OrderByDescending(x => x.AppId);
                            break;

                        case "state_asc":
                            facilities = facilities.OrderBy(x => x.State);
                            break;
                        case "state_desc":
                            facilities = facilities.OrderByDescending(x => x.State);
                            break;
                        case "locationid_asc":
                            facilities = facilities.OrderByDescending(x => x.LocationId);
                            break;
                        case "locationid_desc":
                            facilities = facilities.OrderByDescending(x => x.LocationId);
                            break;

                    }
                }

                return facilities;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Facility Controller / Getting All Facilities :{ErrorMsg}", ex.Message);
                facilityCount = 0;
                return Enumerable.Empty<FacilityViewModel>();
            }
        }

        public async Task<FacilityViewModel> GetFacilityDetail(int facilityId)
        {
            try
            {
                var facility = await context.Facilities.Where(f => f.Id == facilityId).FirstOrDefaultAsync();
                var facilityApplications = await context.FacilityApplications.Where(fa => fa.FacilityId == facility.Id).ToListAsync();

                FacilityViewModel facilityViewModel = new FacilityViewModel();

                if (facility != null)
                {
                    facilityViewModel.Id = facility.Id;
                    facilityViewModel.FacilityID = facility.FacilityID.ToString();
                    facilityViewModel.OrganizationId = facility.OrganizationId.ToString();
                    facilityViewModel.FacilityName = facility.FacilityName;
                    facilityViewModel.Address1 = facility.Address1;
                    facilityViewModel.Address2 = facility.Address2;
                    facilityViewModel.State = facility.State;
                    facilityViewModel.City = facility.City;
                    facilityViewModel.ZipCode = facility.ZipCode;
                    facilityViewModel.IsActive = facility.IsActive;
                    facilityViewModel.HasDeduction= facility.HasDeduction;
                    if (facility.HasDeduction)
                    {
                        var deductionTime=await context.BreakDeductions.Where(c=>c.FacilityId==facility.Id).FirstOrDefaultAsync();
                        if (deductionTime != null)
                        {
                            facilityViewModel.RegularDeduction = deductionTime.RegularTime;
                            facilityViewModel.OverTimeDeduction = deductionTime.OverTime;
                        }
                    }
                    facilityViewModel.LocationId = facility.LocationId;
                    facilityViewModel.LocationDescription = facility.LocationDescription;
                }

                if (facilityApplications != null)
                {
                    facilityViewModel.ApplicationId = new List<string>();
                    foreach (var application in facilityApplications)
                    {
                        if (application.IsActive)
                        {
                            facilityViewModel.ApplicationId.Add(application.ApplicationId.ToString());
                        }
                    }
                }

                return facilityViewModel;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Facility Controller / Getting Facility Details :{ErrorMsg}", ex.Message);
                return null;
            }
        }

        public async Task<string> UpdateFacility(FacilityViewModel facilityViewModel)
        {
            try
            {
                var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                var existingFacility = await context.Facilities.FirstOrDefaultAsync(f => f.Id == facilityViewModel.Id);
                var existingOrganizationFacility = await context.OrganizationFacilities.FirstOrDefaultAsync(of => of.FacilityId == facilityViewModel.Id);
                var facilityApplications = await context.FacilityApplications.Where(fa => fa.FacilityId == facilityViewModel.Id).ToListAsync();
                var facilityUsers = await context.UserOrganizationFacilities.Where(uof => uof.FacilityID == facilityViewModel.Id).ToListAsync();

                if (existingFacility != null)
                {
                    existingFacility.FacilityName = facilityViewModel.FacilityName;
                    existingFacility.Address1 = facilityViewModel.Address1;
                    existingFacility.Address2 = facilityViewModel.Address2;
                    existingFacility.State = facilityViewModel.State;
                    existingFacility.City = facilityViewModel.City;
                    existingFacility.ZipCode = facilityViewModel.ZipCode;
                    existingFacility.OrganizationId = Convert.ToInt32(facilityViewModel.OrganizationId);
                    existingFacility.UpdatedBy = userId;
                    existingFacility.UpdatedDate = DateTime.UtcNow;
                    existingFacility.IsActive = true;
                    existingFacility.HasDeduction = facilityViewModel.HasDeduction;
                    existingFacility.LocationId = facilityViewModel.LocationId;
                    existingFacility.LocationDescription = facilityViewModel.LocationDescription;

                    context.Facilities.Update(existingFacility);

                    if (existingOrganizationFacility != null)
                    {
                        existingOrganizationFacility.UpdatedDate = DateTime.UtcNow;
                        existingOrganizationFacility.UpdatedBy = userId;

                        context.OrganizationFacilities.Update(existingOrganizationFacility);
                    }

                    if (facilityApplications.Count > 0)
                    {
                        // Deactivate existing facility applications
                        foreach (var facilityApplication in facilityApplications)
                        {
                            facilityApplication.IsActive = false;
                            facilityApplication.UpdatedDate = DateTime.UtcNow;
                            facilityApplication.UpdatedBy = userId;
                        }

                        // Add or update facility applications based on the view model data
                        foreach (var applicationId in facilityViewModel.ApplicationId)
                        {
                            if (applicationId != null)
                            {
                                int appId = Convert.ToInt32(applicationId);

                                var existingFacilityApplication = facilityApplications.FirstOrDefault(fa => fa.ApplicationId == appId);
                                if (existingFacilityApplication != null)
                                {
                                    existingFacilityApplication.UpdatedBy = userId;
                                    existingFacilityApplication.UpdatedDate = DateTime.UtcNow;
                                    existingFacilityApplication.IsActive = true;

                                    context.FacilityApplications.Update(existingFacilityApplication);
                                }
                                else
                                {
                                    var newFacilityApplication = new FacilityApplicationModel()
                                    {
                                        FacilityId = existingFacility.Id,
                                        ApplicationId = appId,
                                        CreatedBy = userId,
                                        CreatedDate = DateTime.UtcNow,
                                        IsActive = true
                                    };

                                    context.FacilityApplications.Add(newFacilityApplication);
                                }
                            }
                        }
                    }

                    if (facilityUsers.Count > 0)
                    {
                        foreach (var facilityUser in facilityUsers)
                        {
                            var facilityUserApplications = await context.UserApplications.Where(ua => ua.UserId == facilityUser.UserId).ToListAsync();
                            if (facilityUserApplications != null)
                            {
                                foreach (var facilityUserApplication in facilityUserApplications)
                                {
                                    context.UserApplications.Remove(facilityUserApplication);
                                }
                            }
                            foreach (var applicationId in facilityViewModel.ApplicationId)
                            {
                                if (applicationId != null)
                                {
                                    int appId = Convert.ToInt32(applicationId);

                                    var newFacilityUserApplication = new UserApplication()
                                    {
                                        UserId = facilityUser.UserId,
                                        ApplicationId = appId,
                                    };

                                    context.UserApplications.Add(newFacilityUserApplication);
                                }
                            }
                        }
                    }
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
                Log.Error(ex, "An Error Occured in Facility Controller / Update Existing Facility :{ErrorMsg}", ex.Message);
                return "failed";
            }
        }

        public async Task<string> DeleteFacilityDetail(int facilityId)
        {
            try
            {
                var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                var facility = context.Facilities.Where(f => f.Id == facilityId).FirstOrDefault();
                var existingOrganizationFacility = context.OrganizationFacilities.Where(of => of.FacilityId == facility.Id).FirstOrDefault();
                var facilityApplication = context.FacilityApplications.Where(fa => fa.FacilityId == facility.Id).ToList();
                var facilityUsers = await context.UserOrganizationFacilities.Where(uof => uof.FacilityID == facility.Id).ToListAsync();

                if (facility != null)
                {

                    facility.IsActive = false;
                    facility.UpdatedDate = DateTime.UtcNow;
                    facility.UpdatedBy = userId;
                    context.Facilities.Update(facility);

                    if (existingOrganizationFacility != null)
                    {
                        existingOrganizationFacility.IsActive = false;
                        existingOrganizationFacility.UpdatedDate = DateTime.UtcNow;
                        existingOrganizationFacility.UpdatedBy = userId;
                        context.OrganizationFacilities.Update(existingOrganizationFacility);
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
                            var facilityUserApplications = context.UserApplications.Where(fua => fua.UserId == facilityUser.UserId).ToList();
                            var userRolesDetails = context.UserRoles.Where(ur => ur.UserId == facilityUser.UserId).ToList();
                            var currentUserOtherFacilitiesDetails = context.UserOrganizationFacilities.Where(uof => uof.UserId == facilityUser.UserId && uof.IsActive).ToList();
                            if (currentUserOtherFacilitiesDetails.Count == 1)
                            {
                                if (userDetails != null)
                                {
                                    userDetails.IsActive = false;
                                    userDetails.UpdatedDate = DateTime.UtcNow;
                                    userDetails.ModifiedBy = userId;
                                    context.Users.Update(userDetails);
                                }
                                if (facilityUserApplications.Count > 0)
                                {
                                    foreach (var facilityUserApplication in facilityUserApplications)
                                    {
                                        if (facilityApplication.Any(fa => fa.ApplicationId == facilityUserApplication.ApplicationId))
                                        {
                                            context.UserApplications.Remove(facilityUserApplication);
                                        }
                                    }
                                }
                                if (userRolesDetails.Count > 0)
                                {
                                    foreach (var userRolesDetail in userRolesDetails)
                                    {
                                        context.UserRoles.Remove(userRolesDetail);
                                    }
                                }
                            }
                            facilityUser.IsActive = false;
                            facilityUser.UpdatedDate = DateTime.UtcNow;
                            facilityUser.UpdatedBy = userId;
                            context.UserOrganizationFacilities.Update(facilityUser);
                        }
                    }
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
                Log.Error(ex, "An Error Occured in Facility Controller / Delete Existing Facility :{ErrorMsg}", ex.Message);
                return "failed";
            }

        }

        public async Task<List<FacilityModel>> GetAllFacilitiesByOrgId()
        {
            List<FacilityModel> facility = new List<FacilityModel>();
            try
            {
                var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId != null)
                {
                    var UserData = await _userManager.FindByIdAsync(userId);
                    if (UserData?.UserType == (int)UserTypes.Thinkanew)
                    {
                        facility = (from fa in context.FacilityApplications
                                    join f in context.Facilities on fa.FacilityId equals f.Id
                                    where fa.IsActive && f.IsActive && fa.ApplicationId == (int)ApplicationNameWithId.PBJSnap
                                    select new FacilityModel
                                    {
                                        FacilityID = f.FacilityID,
                                        Id = f.Id,
                                        FacilityName = f.FacilityName
                                    }).ToList();
                    }
                    else
                    {
                        facility = (from u in context.UserOrganizationFacilities
                                    join f in context.Facilities on u.FacilityID equals f.Id
                                    join fa in context.FacilityApplications on u.FacilityID equals fa.FacilityId
                                    where u.UserId == userId && f.IsActive && u.IsActive && fa.ApplicationId == (int)ApplicationNameWithId.PBJSnap
                                    select new FacilityModel
                                    {
                                        FacilityID = f.FacilityID,
                                        Id = f.Id,
                                        FacilityName = f.FacilityName
                                    }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Facility Controller / Get Facilites by Organization :{ErrorMsg}", ex.Message);
            }
            return facility;
        }

        public async Task<List<FacilityModel>> GetAllFacilitiesByOrgId(int orgId)
        {
            List<FacilityModel> facility = new List<FacilityModel>();
            try
            {
                var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId != null)
                {
                    var UserData = await _userManager.FindByIdAsync(userId);
                    if (UserData?.UserType == (int)UserTypes.Thinkanew)
                    {
                        facility = (from fa in context.FacilityApplications
                                    join f in context.Facilities on fa.FacilityId equals f.Id
                                    join o in context.Organizations on f.OrganizationId equals o.OrganizationID
                                    where fa.IsActive && f.IsActive && fa.ApplicationId == (int)ApplicationNameWithId.PBJSnap && f.OrganizationId == orgId
                                    select new FacilityModel
                                    {
                                        FacilityID = f.FacilityID,
                                        Id = f.Id,
                                        FacilityName = f.FacilityName
                                    }).ToList();
                    }
                    else
                    {
                        facility = (from u in context.UserOrganizationFacilities
                                    join f in context.Facilities on u.FacilityID equals f.Id
                                    join fa in context.FacilityApplications on u.FacilityID equals fa.FacilityId
                                    where u.UserId == userId && f.IsActive && u.IsActive && fa.ApplicationId == (int)ApplicationNameWithId.PBJSnap
                                    select new FacilityModel
                                    {
                                        FacilityID = f.FacilityID,
                                        Id = f.Id,
                                        FacilityName = f.FacilityName
                                    }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Facility Controller / Get Facilites by Organization :{ErrorMsg}", ex.Message);
            }
            return facility;
        }

        public List<StateModel> GetStateList()
        {
            try
            {
                return context.States.Select(x => new StateModel()
                {
                    StateID = x.StateID,
                    StateName = x.StateName,
                    StateCode = x.StateCode,
                    DisplayOrder = x.DisplayOrder
                }).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
                return new List<StateModel>();
            }
        }

        public async Task<bool> GetFacilityName(string facilityName, string facilityId)
        {
            try
            {
                var result = await context.Facilities.Where(f => f.FacilityName == facilityName).FirstOrDefaultAsync();
                if (result != null)
                {
                    if (result.FacilityID == facilityId.ToString())
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Facility Controller / Getting Facility Name :{ErrorMsg}", ex.Message);
                return false;
            }
        }

        public async Task<bool> GetFacilityId(string facilityId)
        {
            try
            {
                var result = await context.Facilities.Where(f => f.FacilityID == facilityId).FirstOrDefaultAsync();
                if (result != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Facility Controller / Getting Facility Id:{ErrorMsg}", ex.Message);
                return false;
            }
        }

        public bool CheckRole()
        {
            try
            {
                // Get the logged-in user
                var currentUser = _httpContext.HttpContext.User;
                if (currentUser != null)
                {
                    return currentUser.IsInRole("SuperAdmin");
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Facility Controller / Checking For User Is In SuperAdmin Role :{ErrorMsg}", ex.Message);
                return false;
            }
        }
        public ServiceOrganizationViewModel GetOrganizationServiceDetails(int organizationId)
        {
            ServiceOrganizationViewModel serviceOrganizationViewModel = new();
            try
            {
                var serviceDetails = context.OrganizationServices.Where(o => o.OrganizationId == organizationId && o.ServiceType == Convert.ToInt32(ServiceType.Voice) && o.IsActive).FirstOrDefault();
                if (serviceDetails != null)
                {
                    serviceOrganizationViewModel.OrganizationServiceType = serviceDetails.ServiceType;
                    serviceOrganizationViewModel.OrganizationId = organizationId;
                }
                return serviceOrganizationViewModel;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Facility Controller / Getting Organization Service Details :{ErrorMsg}", ex.Message);
                return serviceOrganizationViewModel;
            }
        }

        public string? GetOrganizationUserDetails()
        {
            try
            {
                // Get the logged-in user
                var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId != null)
                {
                    var userOrganizationDetails = context.UserOrganizationFacilities.Where(o => o.UserId == userId).FirstOrDefault();
                    if (userOrganizationDetails != null)
                    {
                        return userOrganizationDetails.OrganizationID.ToString();
                    }
                    return null;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Facility Controller / Checking For User Is In SuperAdmin Role :{ErrorMsg}", ex.Message);
                return null;
            }
        }

        public bool GetLoggedInUserInfo()
        {
            try
            {
                // Get the logged-in user
                var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                var currentUser = _userManager.Users.Where(u => u.Id == userId).FirstOrDefault();
                if (currentUser != null && currentUser.UserType == Convert.ToInt32(UserTypes.Thinkanew))
                {

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Facility Controller / Geting LoggedIn User Information :{ErrorMsg}", ex.Message);
                return false;
            }
        }

        public async Task<List<FacilityLocationAccess>> GetAllFacilitiesLocationIdByOrgId()
        {
            List<FacilityLocationAccess> facility = new List<FacilityLocationAccess>();
            try
            {
                var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId != null)
                {
                    var UserData = await _userManager.FindByIdAsync(userId);
                    if (UserData?.UserType == (int)UserTypes.Thinkanew)
                    {
                        facility = (from org in context.Organizations
                                    join orgs in context.OrganizationServices on org.OrganizationID equals orgs.OrganizationId
                                    join f in context.Facilities on org.OrganizationID equals f.OrganizationId
                                    where org.IsActive && orgs.IsActive && f.LocationId != "" && f.IsActive && orgs.ServiceType == (int)ServiceType.Data
                                    select new FacilityLocationAccess
                                    {
                                        OrganizationID = org.OrganizationID,
                                        OrganizationName = org.OrganizationName,
                                        MasterAccountId = orgs.MasterAccountId,
                                        SubAccountId = orgs.SubAccountId,
                                        FacilityID = f.Id,
                                        FacilityName = f.FacilityName,
                                        LocationId = f.LocationId
                                    }).ToList();
                    }
                    else
                    {
                        facility = (from org in context.Organizations
                                    join orgs in context.OrganizationServices on org.OrganizationID equals orgs.OrganizationId
                                    join f in context.Facilities on org.OrganizationID equals f.OrganizationId
                                    where org.IsActive && orgs.IsActive && f.LocationId != "" && f.IsActive && orgs.ServiceType == (int)ServiceType.Data
                                    select new FacilityLocationAccess
                                    {
                                        OrganizationID = org.OrganizationID,
                                        OrganizationName = org.OrganizationName,
                                        MasterAccountId = orgs.MasterAccountId,
                                        SubAccountId = orgs.SubAccountId,
                                        FacilityID = f.Id,
                                        FacilityName = f.FacilityName,
                                        LocationId = f.LocationId
                                    }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Facility Controller / Get Facilites by Organization :{ErrorMsg}", ex.Message);
            }
            return facility;
        }
    }
}
