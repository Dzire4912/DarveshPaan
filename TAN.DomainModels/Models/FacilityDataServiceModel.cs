using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Models
{
    public class FacilityDataServiceModel
    {
        public long Id { get; set; }
        public string? FacilityName { get; set; } 
        public int FacilityID { get; set; }
        public string? LocationId { get; set; }
        public string? AccountNumber { get; set; }
        public string? ProviderName { get; set; }
        public string? LocalIP { get; set; }
        public string? SupportNumber { get; set; }
        public string? ServiceName { get; set; }
        public string? ContractStatus { get; set; }
        public string? ContractDocument { get; set;}
    }
}
