using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Models;

namespace TAN.Repository.Abstractions
{
    public interface ICensus : IRepository<Census>
    {
       List<CensusData> GetCencusList(int PageSize,int PageNo,out int TotalCount, CensusViewRequest cencusViewRequest, string sortOrder);
        Task<bool> AddCencus(Census cencus);
        Task<bool> DeleteCencus(int CencusId);
       DataTable GetCencusByFacilityAndQuarter(int FacilityId, int ReportQuarter, int Year);
    }
}
