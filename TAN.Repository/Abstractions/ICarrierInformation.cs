using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using TAN.DomainModels.ViewModels;

namespace TAN.Repository.Abstractions
{
    public interface ICarrierInformation : IRepository<CarrierInfomation>
    {
        Task<string> AddCarrier(CarrierInfomationViewModel carrier);
        IEnumerable<CarrierInfomationViewModel> GetAllCarriers(int skip, int take, string? searchValue, string sortOrder, out int carrierCount);
        Task<bool> GetCarrierName(string carrierName, int carrierId);
        Task<CarrierInfomationViewModel> GetCarrierDetail(int carrierId);
        Task<string> UpdateCarrier(CarrierInfomationViewModel carrier);
        Task<string> DeleteCarrierDetail(int carrierId);
    }
}
