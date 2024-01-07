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
    public interface IDataServiceType : IRepository<DataServiceType>
    {
        Task<Response> AddDataServiceType(DataServiceTypeViewModel dataServiceTypeViewModel);
        IEnumerable<DataServiceTypeViewModel> GetAllDataServiceType(string? searchValue, string sortOrder, string skip, string take, out int dataServiceTypeCount);
        Task<DataServiceTypeViewModel> GetDataServiceTypeById(string id);
        Task<Response> DeleteDataServiceTypeById(string id);
    }
}
