﻿using Microsoft.EntityFrameworkCore;
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
    public class KronosDataTrackingRepository:Repository<KronosDataTrack>,IkronosDataTracking
    {
        private DatabaseContext context
        {
            get
            {
                return db as DatabaseContext;
            }
        }
        public KronosDataTrackingRepository(DbContext db)
        {
            this.db = db;
        }
    }
}
