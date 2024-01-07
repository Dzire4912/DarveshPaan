using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.ApplicationCore;
using TAN.DomainModels.Entities;
using TAN.Repository.Implementations;

namespace TAN.Repository.Abstractions
{
    [ExcludeFromCodeCoverage]
    public class TANEmployeeRepository:Repository<TANEmployeeDetails>,ITANEmployeeRepository
    {
        private DatabaseContext context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public TANEmployeeRepository(DbContext db)
        {
            this.db = db;
        }
    }
}
