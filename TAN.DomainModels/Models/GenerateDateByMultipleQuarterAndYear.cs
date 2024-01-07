using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Helpers;

namespace TAN.DomainModels.Models
{
    public class GenerateDateByMultipleQuarterAndYear
    {
        public GenerateDateByMultipleQuarterAndYear(int startQuarter, int endQuarter, int startYear, int endYear)
        {
            GetQuarterStartDate(startQuarter, startYear);
            GetQuarterEndDate(endQuarter, endYear);
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

        public void GetQuarterStartDate(int quarter, int year)
        {
            if(quarter == (int)ReportingQuarter.Jan_Mar)
            {
                year--;
            }
            switch (quarter)
            {
                case 1:
                    StartDate = Convert.ToDateTime(year.ToString() + "-10-01");
                    break;
                case 2:
                    StartDate = Convert.ToDateTime(year.ToString() + "-01-01");
                    break;
                case 3:
                    StartDate = Convert.ToDateTime(year.ToString() + "-04-01");
                    break;
                case 4:
                    StartDate = Convert.ToDateTime(year.ToString() + "-07-01");
                    break;
                default:
                    break;
            }
        }

        public void GetQuarterEndDate(int quarter, int year)
        {
            switch (quarter)
            {
                case 1:
                    EndDate = Convert.ToDateTime(year.ToString() + "-12-31");
                    break;
                case 2:
                    EndDate = Convert.ToDateTime(year.ToString() + "-03-31");
                    break;
                case 3:
                    EndDate = Convert.ToDateTime(year.ToString() + "-06-30");
                    break;
                case 4:
                    EndDate = Convert.ToDateTime(year.ToString() + "-09-30");
                    break;
                default:
                    break;
            }
        }
    }
}
