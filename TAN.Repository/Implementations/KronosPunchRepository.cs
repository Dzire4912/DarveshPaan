using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.ApplicationCore;
using TAN.ApplicationCore.Migrations;
using TAN.DomainModels.Entities;
using TAN.Repository.Abstractions;

namespace TAN.Repository.Implementations
{
    [ExcludeFromCodeCoverage]
    public class KronosPunchRepository : Repository<KronosPunchExportModel>,IKronosPunchRepository
    {

        private DatabaseContext Context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public KronosPunchRepository(DbContext db)
        {
            this.db = db;
        }

       public async Task<List<Timesheet>> ConvertedKronosData(List<KronosPunchExportModel> exportModels)
        {
            List<Timesheet> result = new List<Timesheet>();
            
            return result;
        }

        public async Task<int> GetCurrentQuarter()
        {
            int result = 0;           
            var currentMonth = DateTime.Now.Month;

            var matchingMonth = await Context.Months.FirstOrDefaultAsync(e => e.Id == currentMonth);

            if (matchingMonth != null)
            {
                result = matchingMonth.Quarter;
            }

            return result;
        }
    }
}
