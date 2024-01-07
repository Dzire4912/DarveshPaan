using Microsoft.EntityFrameworkCore;
using TAN.ApplicationCore;
using TAN.DomainModels.Entities;
using TAN.Repository.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace TAN.Repository.Implementations
{
    [ExcludeFromCodeCoverage]
    public class ModuleRepository : Repository<Module>, IModuleRepository
    {
        private DatabaseContext context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public ModuleRepository(DbContext db)
        {
            this.db = db;
        }

    }
}
