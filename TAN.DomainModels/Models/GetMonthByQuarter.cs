using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Models
{
    [ExcludeFromCodeCoverage]
    public class GetMonthByQuarter
    {
        public GetMonthByQuarter(int Quarter, int Month)
        {
            GetMonth(Quarter, Month);
        }
        public List<int>? Month
        {
            get;
            private set;
        }
        public void GetMonth(int Quarter, int month)
        {
            try
            {
                Month = new List<int>();
                if (month == 0)
                {
                    switch (Quarter)
                    {
                        case 1:
                            Month.Add(0);
                            Month.Add(10);
                            Month.Add(11);
                            Month.Add(12);
                            break;
                        case 2:
                            Month.Add(0);
                            Month.Add(1);
                            Month.Add(2);
                            Month.Add(3);
                            break;
                        case 3:
                            Month.Add(0);
                            Month.Add(4);
                            Month.Add(5);
                            Month.Add(6);
                            break;
                        case 4:
                            Month.Add(0);
                            Month.Add(7);
                            Month.Add(8);
                            Month.Add(9);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    Month.Add(month);
                }
            }
            catch (Exception)
            {
                //Log
            }
        }
    }
}
