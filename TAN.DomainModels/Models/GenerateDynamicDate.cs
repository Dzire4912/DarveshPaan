using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Models
{
    public class GenerateDynamicDate
    {
        public static IEnumerable<DynamicDate> GenerateDynamicDates(DateTime startDate, DateTime endDate)
        {
            var dynamicRecords = new List<DynamicDate>();

            DateTime currentDate = startDate;
            int level = 1;

            while (currentDate <= endDate)
            {
                dynamicRecords.Add(new DynamicDate { myDate = currentDate });
                currentDate = currentDate.AddDays(1);
                level++;
            }

            return dynamicRecords;
        }
        public class DynamicDate
        {
            public DateTime myDate { get; set; }
        }
    }
}
