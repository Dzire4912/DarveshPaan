using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.ApplicationCore;
using TAN.DomainModels.Entities;
using TAN.Repository.Abstractions;

namespace TAN.Repository.Implementations
{
    public class InvoiceDataRepository : Repository<InvoiceData>, IInvoiceData
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly Microsoft.AspNetCore.Identity.UserManager<AspNetUser> _userManager;
        private DatabaseContext Context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public InvoiceDataRepository(DbContext db, IHttpContextAccessor httpContext, UserManager<AspNetUser> userManager)
        {
            this.db = db;
            _httpContext = httpContext;
            _userManager = userManager;
        }

    }
}
