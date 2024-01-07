using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.ApplicationCore;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Models;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using System.Web.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace TAN.Repository.Implementations
{
    [ExcludeFromCodeCoverage]
    public class DataServiceTypeRepository : Repository<DataServiceType>, IDataServiceType
    {
        private readonly IHttpContextAccessor _httpContext;
        private DatabaseContext Context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public DataServiceTypeRepository(DbContext db, IHttpContextAccessor httpContext)
        {
            this.db = db;
            _httpContext = httpContext;
        }

        public async Task<Response> AddDataServiceType(DataServiceTypeViewModel dataServiceTypeViewModel)
        {
            Response response = new Response();
            try
            {
                const string noDataId= "0";
                if (dataServiceTypeViewModel != null)
                {
                    var _dataServiceType = Context.DataServiceType.Where(c => c.ServiceName == dataServiceTypeViewModel.DataServiceTypeName && c.IsActive).FirstOrDefault();
                    if (_dataServiceType != null)
                    {
                        response.StatusCode = Convert.ToInt32(ServiceTypeStatus.AllReadyExist);
                        response.Message = "Service name exist.";
                        return response;
                    }
                    
                    if (dataServiceTypeViewModel.DataServiceTypeId == noDataId && !string.IsNullOrEmpty(dataServiceTypeViewModel.DataServiceTypeName))
                    {
                        var dataServicetype = new DataServiceType()
                        {
                            ServiceName = dataServiceTypeViewModel.DataServiceTypeName,
                            IsActive = true
                        };
                        await Context.DataServiceType.AddAsync(dataServicetype);
                        await Context.SaveChangesAsync();
                        response.StatusCode = Convert.ToInt32(ServiceTypeStatus.Success);
                        response.Message = "Service type is added";
                        return response;
                    }
                    else if (dataServiceTypeViewModel.DataServiceTypeId == noDataId && string.IsNullOrEmpty(dataServiceTypeViewModel.DataServiceTypeName))
                    {
                        response.StatusCode = Convert.ToInt32(ServiceTypeStatus.Error);
                        response.Message = "Empty string is not allowed";
                    }
                    else
                    {
                        //update
                        var data = Context.DataServiceType.Where(x => x.Id == Convert.ToInt32(dataServiceTypeViewModel.DataServiceTypeId)).AsNoTracking().FirstOrDefault();
                        if (data != null)
                        {
                            data.ServiceName = dataServiceTypeViewModel.DataServiceTypeName;
                            Context.DataServiceType.Update(data);
                            await Context.SaveChangesAsync();
                            response.StatusCode = Convert.ToInt32(ServiceTypeStatus.DataUpdated);
                            response.Message = "Service type is updated";
                            return response;
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured AddDataServiceType :{ErrorMsg}", ex.Message);
            }
            return response;
        }
        public IEnumerable<DataServiceTypeViewModel> GetAllDataServiceType(string? searchValue, string sortOrder, string skip, string take, out int dataServiceTypeCount)
        {
            IEnumerable<DataServiceTypeViewModel> dataServiceTypeViewModels = null;
            dataServiceTypeCount = 0;
            try
            {
                var lowercaseSearchValue = searchValue?.ToLower();

                dataServiceTypeViewModels = (from d in Context.DataServiceType
                                             where d.IsActive == true &&
                                             (string.IsNullOrEmpty(lowercaseSearchValue) ||
                                             d.ServiceName.ToLower().Contains(lowercaseSearchValue) || d.Id.ToString().Contains(lowercaseSearchValue))
                                             orderby d.Id

                                             select new DataServiceTypeViewModel
                                             {
                                                 DataServiceTypeId = d.Id.ToString(),
                                                 DataServiceTypeName = d.ServiceName,
                                             }).AsEnumerable();

                if (dataServiceTypeViewModels != null && dataServiceTypeViewModels.Count() > 0)
                {

                    dataServiceTypeCount = dataServiceTypeViewModels.Count();
                    dataServiceTypeViewModels = dataServiceTypeViewModels.Skip(Convert.ToInt32(skip)).Take(Convert.ToInt32(take));

                    if (!string.IsNullOrEmpty(sortOrder))
                    {
                        switch (sortOrder)
                        {
                            case "dataservicetypename_asc":
                                dataServiceTypeViewModels = dataServiceTypeViewModels.OrderBy(x => x.DataServiceTypeName);
                                break;
                            case "dataservicetypename_desc":
                                dataServiceTypeViewModels = dataServiceTypeViewModels.OrderByDescending(x => x.DataServiceTypeName);
                                break;
                        }
                    }
                }

                return dataServiceTypeViewModels;
            }
            catch (Exception ex)
            {
                dataServiceTypeCount = 0;
                Log.Error(ex, "An Error Occured in GetAllDataServiceType method :{ErrorMsg} ", ex.Message);
            }
            return dataServiceTypeViewModels;

        }

        public async Task<DataServiceTypeViewModel> GetDataServiceTypeById(string id)
        {
            var dataById = Context.DataServiceType.Where(d => d.Id == Convert.ToInt32(id) && d.IsActive == true).FirstOrDefault();
            DataServiceTypeViewModel dataServiceTypeViewModel = new DataServiceTypeViewModel();
            try
            {
                if (dataById != null)
                {
                    dataServiceTypeViewModel.DataServiceTypeId = dataById.Id.ToString();
                    dataServiceTypeViewModel.DataServiceTypeName = dataById.ServiceName;
                    dataServiceTypeViewModel.IsActive = dataById.IsActive;
                }
                return dataServiceTypeViewModel;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in GetDataServiceTypeById :{ErrorMsg}", ex.Message);
            }
            return dataServiceTypeViewModel;
        }

        public async Task<Response> DeleteDataServiceTypeById(string id)
        {
            Response response = new Response();
            try
            {
                var data = Context.DataServiceType.Where(x => x.Id == Convert.ToInt32(id)).AsNoTracking().FirstOrDefault();
                if (data != null)
                {
                    data.IsActive= false;
                    Context.DataServiceType.Update(data);
                    await Context.SaveChangesAsync();
                    response.StatusCode = Convert.ToInt32(ServiceTypeStatus.DataDeleted);
                    response.Message = "Data is Deleted";
                    return response;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in DeleteDataServiceTypeById :{ErrorMsg}", ex.Message);
            }
            return response;
        }

    }

}
