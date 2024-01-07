using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Models
{
    public class BandwidthAccessList
    {
        public string? OrganizationName { get; set; }
        public string? MasterAccountId { get; set; }
        public string? SubAccountId { get; set; }
        public string? FacilityName { get; set; }
        public string? FacilityID { get; set; }
        public string? LocationId { get; set; }
    }
}
