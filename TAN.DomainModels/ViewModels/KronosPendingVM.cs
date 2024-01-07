using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;

namespace TAN.DomainModels.ViewModels
{
    public class KronosPendingVM
    {
        public List<KronosPendingRecords> kronosPendingdata {  get; set; }
        public List<Itemlist> Orglist { get; set; }
        public List<Itemlist> facilityList { get; set; }
        public List<Itemlist> EmployeeList { get; set; }
        public List<Itemlist> QuarterList { get; set; }

    }
}
