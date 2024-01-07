using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using TAN.ApplicationCore;
using TAN.DomainModels.Entities;
using TAN.Repository.Abstractions;

namespace TAN.Repository.Implementations
{
    [ExcludeFromCodeCoverage]
    public class PayTypeCodesRepository: Repository<PayTypeCodes>, IPayTypeCodes
    {
        private DatabaseContext Context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public PayTypeCodesRepository(DbContext db)
        {
            this.db = db;
        }
        public async Task<List<PayTypeCodes>> GetAllPayTypeCodes()
        {
            List<PayTypeCodes> payTypeCodes = new List<PayTypeCodes>();
            try
            {
                payTypeCodes = await Context.PayTypeCodes.Where(x => x.IsActive).ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetAllPayTypeCodes:{ErrorMsg}", ex.Message);
            }
            return payTypeCodes;
        }
    }
}
