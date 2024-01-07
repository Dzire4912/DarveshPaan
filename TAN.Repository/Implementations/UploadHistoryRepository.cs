using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
    public class UploadHistoryRepository : Repository<UploadHistory>, IUploadHistoryRepository
    {
        private readonly IHttpContextAccessor _httpContext;
        private DatabaseContext context
        {
            get
            {
                return db as DatabaseContext;
            }
        }
        public UploadHistoryRepository(DbContext db, IHttpContextAccessor httpContext)
        {
            _httpContext = httpContext;
            this.db = db;
        }
    }
}
