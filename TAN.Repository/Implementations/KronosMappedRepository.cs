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
    public class KronosMappedRepository:Repository<KronosTimeSheetMapped>,IKronosMappedRepository
    {
        private DatabaseContext Context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public KronosMappedRepository(DbContext db)
        {
            this.db = db;
           
        }

        //public  List<KronosTimeSheetMapped> callmySP(FormattableString Name)
        //{
        //    var result= db.Database.SqlQuery<KronosTimeSheetMapped>(Name);
        //    return result.ToList();
        //}

    }
}
