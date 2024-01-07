using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Models;

namespace TAN.Repository.Abstractions
{
    public interface IDataService : IRepository<DataService>
    {
        Task<bool> AddDataService(DataService dataService);
        List<FacilityDataServiceModel> GetDataServiceList(GetDataServiceListRequest listRequest, out int TotalCount);
        Task<bool> DeleteDataService(string Id);
        Task<DataServiceRequest> GetDataServiceById(string Id);
        Task<bool> CheckFileName(CheckDataServiceFileName check);
        Task<bool> CheckAccountNumber(CheckAccountNumberRequest check);
    }
}
