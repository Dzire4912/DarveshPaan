using Microsoft.AspNetCore.Http;
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
    public class BandwidthCallLogsRepo : Repository<BandwidthCallLogs>, IBandwidthCallLogs
    {
        private readonly IHttpContextAccessor _httpContext;
        private DatabaseContext Context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public BandwidthCallLogsRepo(DbContext db, IHttpContextAccessor httpContext)
        {
            this.db = db;
            _httpContext = httpContext;
        }

        public async Task<bool> AddBandwidthLogs(BandwidthCallLogs callLogs)
        {
            try
            {
                if (callLogs != null)
                {
                    await Context.BandwidthCallLogs.AddAsync(callLogs);
                    await Context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured BandwidthCallLogsRepo/AddBandwidthLogs :{ErrorMsg}", ex.Message);
            }
            return false;
        }
    }
}
