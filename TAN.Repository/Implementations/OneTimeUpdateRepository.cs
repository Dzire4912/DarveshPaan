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
    public class OneTimeUpdateRepository:Repository<KronosOneTimeUpdate>,IOneTimeDeduction
    {
        private DatabaseContext Context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public OneTimeUpdateRepository(DbContext db)
        {
            this.db = db;

        }

       
    }
}
