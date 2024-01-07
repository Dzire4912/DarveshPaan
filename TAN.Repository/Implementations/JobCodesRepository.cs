using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.ApplicationCore;
using TAN.DomainModels.Entities;
using TAN.Repository.Abstractions;

namespace TAN.Repository.Implementations
{
    [ExcludeFromCodeCoverage]
    public class JobCodesRepository : Repository<JobCodes>, IJobCodes
    {
        private DatabaseContext Context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public JobCodesRepository(DbContext db)
        {
            this.db = db;
        }

        public async Task<List<JobCodes>> GetAllJobCodes()
        {
            List<JobCodes> JobCode = new List<JobCodes>();
            try
            {
                JobCode = await Context.JobCodes.Where(x => x.IsActive).ToListAsync();    
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetAllJobCodes:{ErrorMsg}", ex.Message);
            }
            return JobCode;
        }
    }
}
