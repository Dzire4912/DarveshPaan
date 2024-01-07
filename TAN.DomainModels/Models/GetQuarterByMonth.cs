using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Helpers;

namespace TAN.DomainModels.Models
{
    public class GetQuarterByMonth
    {
        public GetQuarterByMonth(int month)
        {
            GetQuarter(month);
        }
        public int Quarter
        {
            get;
            private set;
        }

        public int GetQuarter(int month)
        {
            Quarter = 0;
            switch (month)
            {
                case 1:
                case 2:
                case 3:
                    Quarter = (int)ReportingQuarter.Apr_Jun;
                    break;
                case 4:
                case 5:
                case 6:
                    Quarter = (int)ReportingQuarter.Apr_Jun;
                    break;
                case 7:
                case 8:
                case 9:
                    Quarter = (int)ReportingQuarter.Jul_Sept;
                    break;
                case 10:
                case 11:
                case 12:
                    Quarter = (int)ReportingQuarter.Oct_Dec;
                    break;
                default:
                    break;
            }
            return Quarter;
        }
    }
}
