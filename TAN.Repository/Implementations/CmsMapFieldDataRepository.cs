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
    public class CmsMapFieldDataRepository : Repository<CMSMappingFieldData>, ICmsMapFieldData
    {
        private DatabaseContext Context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public CmsMapFieldDataRepository(DbContext db)
        {
            this.db = db;
        }

        public async Task<List<CMSMappingFieldData>> GetAllCMSMappingFieldData()
        {
            List<CMSMappingFieldData> model = new List<CMSMappingFieldData>();
            try
            {
                model = await Context.CMSMappingFieldData.Where(x => x.IsActive == true).ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error("GetAllCMSMappingFieldData ", ex.ToString());
                throw;
            }
            return model;
        }
    }
}
