using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.Design;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TAN.ApplicationCore;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Models;
using TAN.Repository.Abstractions;

namespace TAN.Repository.Implementations
{
    [ExcludeFromCodeCoverage]
    public class DataServiceRepository : Repository<DataService>, IDataService
    {
        private readonly IHttpContextAccessor _httpContext;
        private DatabaseContext Context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public DataServiceRepository(DbContext db, IHttpContextAccessor httpContext)
        {
            this.db = db;
            _httpContext = httpContext;
        }

        public async Task<bool> AddDataService(DataService dataService)
        {
            try
            {
                if (dataService != null)
                {
                    if(dataService.Id != 0)
                    {
                        Context.DataService.Update(dataService);
                        await Context.SaveChangesAsync();
                        return true;
                    }
                    else
                    {
                        await Context.DataService.AddAsync(dataService);
                        await Context.SaveChangesAsync();
                        return true;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured AddDataService :{ErrorMsg}", ex.Message);
            }
            return false;
        }

        public List<FacilityDataServiceModel> GetDataServiceList(GetDataServiceListRequest listRequest, out int TotalCount)
        {
            List<FacilityDataServiceModel> list = new List<FacilityDataServiceModel>();
            TotalCount = 0;
            try
            {
                if (listRequest != null)
                {
                    list = (from ds in Context.DataService
                            join f in Context.Facilities on ds.FacilityId equals f.Id
                            join dst in Context.DataServiceType on ds.ServicesId equals dst.Id
                            where f.IsActive && ds.IsActive && dst.IsActive && ds.FacilityId == Convert.ToInt32(listRequest.facilityId)
                            && (listRequest.serviceId == null || ds.ServicesId == Convert.ToInt32(listRequest.serviceId))
                            select new FacilityDataServiceModel
                            {
                                Id = ds.Id,
                                FacilityName = f.FacilityName,
                                FacilityID = f.Id,
                                LocationId = f.LocationId,
                                AccountNumber = ds.AccountNumber,
                                ProviderName = ds.ProviderName,
                                LocalIP = ds.LocalIP,
                                SupportNumber = ds.SupportNumber,
                                ServiceName = dst.ServiceName,
                                ContractStatus = ds.ContractStatus ? "Active" : "Inactive",
                                ContractDocument = (ds.ContractDocument == null) ? "NA" : ds.ContractDocument
                            }).ToList();
                    if (list != null && list.Count > 0)
                    {
                        TotalCount = list.Count;
                        list = list.Skip(listRequest.skip).Take(listRequest.PageSize).ToList();
                        if (!string.IsNullOrEmpty(listRequest.search?.ToLower()))
                        {
                            list = ApplySearch(list, listRequest.search.ToLower());
                        }
                        if (!string.IsNullOrEmpty(listRequest.sortOrder))
                        {
                            switch (listRequest.sortOrder)
                            {
                                case "facilityname_asc":
                                    list = list.OrderBy(x => x.FacilityName).ToList();
                                    break;
                                case "facilityname_desc":
                                    list = list.OrderByDescending(x => x.FacilityName).ToList();
                                    break;
                                case "accountnumber_asc":
                                    list = list.OrderBy(x => x.AccountNumber).ToList();
                                    break;
                                case "accountnumber_desc":
                                    list = list.OrderByDescending(x => x.AccountNumber).ToList();
                                    break;
                                case "servicename_asc":
                                    list = list.OrderBy(x => x.ServiceName).ToList();
                                    break;
                                case "servicename_desc":
                                    list = list.OrderByDescending(x => x.ServiceName).ToList();
                                    break;
                                case "providername_asc":
                                    list = list.OrderBy(x => x.ProviderName).ToList();
                                    break;
                                case "providername_desc":
                                    list = list.OrderByDescending(x => x.ProviderName).ToList();
                                    break;
                                case "contractdocument_asc":
                                    list = list.OrderBy(x => x.ContractDocument).ToList();
                                    break;
                                case "contractdocument_desc":
                                    list = list.OrderByDescending(x => x.ContractDocument).ToList();
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetDataServiceList :{ErrorMsg}", ex.Message);
            }
            return list;
        }

        private List<FacilityDataServiceModel> ApplySearch(List<FacilityDataServiceModel> list, string lowercaseSearchValue)
        {
            try
            {
                list = list.Where(e => e.ProviderName.ToLower().ToString().Contains(lowercaseSearchValue) ||
                e.FacilityName.ToLower().ToString().Contains(lowercaseSearchValue) ||
                e.ServiceName.ToLower().ToString().Contains(lowercaseSearchValue) ||
                e.ContractDocument.ToLower().ToString().Contains(lowercaseSearchValue) ||
                e.AccountNumber.ToLower().ToString().Contains(lowercaseSearchValue)).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetDataServiceList ApplySearch :{ErrorMsg}", ex.Message);
            }

            return list;
        }

        public async Task<bool> DeleteDataService(string Id)
        {
            try
            {
                if (Id != null && Id != "")
                {
                    var data = await Context.DataService.Where(x => x.Id == Convert.ToInt64(Id)).AsNoTracking().FirstOrDefaultAsync();
                    if (data != null) {
                        var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                        Context.DataService.Where(x => x.Id == Convert.ToInt64(Id))
                         .ExecuteUpdate(e => e.SetProperty(s => s.IsActive, s => false)
                         .SetProperty(s => s.UpdateBy, s => userId)
                         .SetProperty(s => s.UpdateDate, s => DateTime.Now));
                        await Context.SaveChangesAsync();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured DeleteDataService :{ErrorMsg}", ex.Message);
            }
            return false;
        }

        public async Task<DataServiceRequest> GetDataServiceById(string Id)
        {
            DataServiceRequest request = new DataServiceRequest();
            try
            {
                if (Id != null && Id != "")
                {
                    var data = await Context.DataService.Where(x => x.Id == Convert.ToInt64(Id)).AsNoTracking().FirstOrDefaultAsync();
                    if (data != null)
                    {
                        request.Id = Convert.ToString(data.Id);
                        request.AccountNumber = data.AccountNumber;
                        request.SupportNumber = data.SupportNumber;
                        request.ServicesId = data.ServicesId;
                        request.LocalIP = data.LocalIP;
                        request.ContractStatus = data.ContractStatus;
                        request.ContractEndDate = data.ContractEndDate;
                        request.ProviderName = data.ProviderName;
                        request.FacilityId = Convert.ToString(data.FacilityId);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetDataServiceById :{ErrorMsg}", ex.Message);
            }
            return request;
        }

        public async Task<bool> CheckAccountNumber(CheckAccountNumberRequest check)
        {
            bool result = false;
            try
            {
                if (check != null)
                {
                    if (check.DataServiceId != 0)
                    {
                        result = Context.DataService.Where(x => x.AccountNumber == check.AccountNo && x.IsActive && x.FacilityId == check.FacilityId && x.Id != check.DataServiceId).AsNoTracking().Any();
                    }
                    else
                    {
                        result = Context.DataService.Where(x => x.AccountNumber == check.AccountNo && x.IsActive && x.FacilityId == check.FacilityId).AsNoTracking().Any();
                    }
                    if (!result)
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured CheckAccountNumber :{ErrorMsg}", ex.Message);
            }
            return true;
        }

        public async Task<bool> CheckFileName(CheckDataServiceFileName check)
        {
            try
            {
                if (!Context.DataService.Where(x => x.ContractDocument == check.filename && x.IsActive && x.FacilityId == check.FacilityId).AsNoTracking().Any())
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetDataServiceById :{ErrorMsg}", ex.Message);
            }
            return true;
        }

    }
}
