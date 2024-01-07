using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.ApplicationCore;

using TAN.DomainModels.Entities;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;


namespace TAN.Repository.Implementations
{
    [ExcludeFromCodeCoverage]
    public class KronosFacilityRepository:Repository<KronosFacilities>,IkronosFacility
    {
        private DatabaseContext Context
        {
            get
            {
                return db as DatabaseContext;
            }
        }
        public KronosFacilityRepository(DbContext db)
        {
            this.db = db;

        }
        public  void DeleteTimesheetFacilities()
        {
            List<KronosFacilities> facilityList = new List<KronosFacilities>();
            var deleteItems = (from kf in Context.kronosFacilities
                               join af in Context.Facilities on kf.FacilityId equals af.FacilityID
                               join ts in Context.Timesheet on af.Id equals ts.FacilityId
                               where af.FacilityID == kf.FacilityId 

                               select  af.FacilityID).Distinct().AsNoTracking().ToList();

            foreach (var item in deleteItems)
            {
                var removeItem = Context.kronosFacilities.FirstOrDefault(x => x.FacilityId == item);
                if (removeItem != null)
                {
                    facilityList.Add(removeItem);
                }
                
            }
            if (facilityList.Count>0)
            {
                Context.kronosFacilities.RemoveRange(facilityList);
                Context.SaveChanges();
            }

        }
    }
}
