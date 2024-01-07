using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;

namespace TAN.Repository.Abstractions
{
    public interface IPayTypeCodes : IRepository<PayTypeCodes>
    {
        Task<List<PayTypeCodes>> GetAllPayTypeCodes();
    }
}
