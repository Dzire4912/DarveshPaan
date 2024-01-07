using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Models
{
    [ExcludeFromCodeCoverage]
    public class GenerateDatesByYearQuarterMonth
    {
        public GenerateDatesByYearQuarterMonth(int Quarter, int Month, int year)
        {
            GetQuarterDate(Quarter, Month, year);
        }
        public DateTime StartDate
        {
            get;
            private set;
        }

        public DateTime EndDate
        {
            get;
            private set;
        }
        public void GetQuarterDate(int Quarter, int month, int year)
        {
            switch (Quarter)
            {
                case 1:
                    if (month == 0)
                    {
                        StartDate = Convert.ToDateTime(year.ToString() + "-10-01");
                        EndDate = Convert.ToDateTime(year.ToString() + "-12-31");
                    }
                    else
                    {
                        GetMonthDate(month, year);
                    }
                    break;
                case 2:
                    if (month == 0)
                    {
                        StartDate = Convert.ToDateTime(year.ToString() + "-01-01");
                        EndDate = Convert.ToDateTime(year.ToString() + "-03-31");
                    }
                    else
                    {
                        GetMonthDate(month, year);
                    }
                    break;
                case 3:
                    if (month == 0)
                    {
                        StartDate = Convert.ToDateTime(year.ToString() + "-04-01");
                        EndDate = Convert.ToDateTime(year.ToString() + "-06-30");
                    }
                    else
                    {
                        GetMonthDate(month, year);
                    }
                    break;
                case 4:
                    if (month == 0)
                    {
                        StartDate = Convert.ToDateTime(year.ToString() + "-07-01");
                        EndDate = Convert.ToDateTime(year.ToString() + "-09-30");
                    }
                    else
                    {
                        GetMonthDate(month, year);
                    }
                    break;
                default:
                    break;
            }
        }

        public void GetMonthDate(int Month, int year)
        {
            switch (Month)
            {
                case 1:
                    StartDate = Convert.ToDateTime(year.ToString() + "-01-01");
                    EndDate = Convert.ToDateTime(year.ToString() + "-01-31");
                    break;
                case 2:
                    StartDate = Convert.ToDateTime(year.ToString() + "-02-01");
                    EndDate = (year % 4 == 0) ? Convert.ToDateTime(year.ToString() + "-02-29") : Convert.ToDateTime(year.ToString() + "-02-28");
                    break;
                case 3:
                    StartDate = Convert.ToDateTime(year.ToString() + "-03-01");
                    EndDate = Convert.ToDateTime(year.ToString() + "-03-31");
                    break;
                case 4:
                    StartDate = Convert.ToDateTime(year.ToString() + "-04-01");
                    EndDate = Convert.ToDateTime(year.ToString() + "-04-30");
                    break;
                case 5:
                    StartDate = Convert.ToDateTime(year.ToString() + "-05-01");
                    EndDate = Convert.ToDateTime(year.ToString() + "-05-31");
                    break;
                case 6:
                    StartDate = Convert.ToDateTime(year.ToString() + "-06-01");
                    EndDate = Convert.ToDateTime(year.ToString() + "-06-30");
                    break;
                case 7:
                    StartDate = Convert.ToDateTime(year.ToString() + "-07-01");
                    EndDate = Convert.ToDateTime(year.ToString() + "-07-31");
                    break;
                case 8:
                    StartDate = Convert.ToDateTime(year.ToString() + "-08-01");
                    EndDate = Convert.ToDateTime(year.ToString() + "-08-31");
                    break;
                case 9:
                    StartDate = Convert.ToDateTime(year.ToString() + "-09-01");
                    EndDate = Convert.ToDateTime(year.ToString() + "-09-30");
                    break;
                case 10:
                    StartDate = Convert.ToDateTime(year.ToString() + "-10-01");
                    EndDate = Convert.ToDateTime(year.ToString() + "-10-31");
                    break;
                case 11:
                    StartDate = Convert.ToDateTime(year.ToString() + "-11-01");
                    EndDate = Convert.ToDateTime(year.ToString() + "-11-30");
                    break;
                case 12:
                    StartDate = Convert.ToDateTime(year.ToString() + "-12-01");
                    EndDate = Convert.ToDateTime(year.ToString() + "-12-31");
                    break;
                default:
                    break;
            }
        }
    }
}
