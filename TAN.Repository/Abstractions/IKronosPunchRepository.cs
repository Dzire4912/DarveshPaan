using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using TAN.Repository.Implementations;

namespace TAN.Repository.Abstractions
{
    public interface IKronosPunchRepository : IRepository<KronosPunchExportModel>
    {
        Task<List<Timesheet>> ConvertedKronosData(List<KronosPunchExportModel> exportModels);
        Task<int> GetCurrentQuarter();
        
    }
}
