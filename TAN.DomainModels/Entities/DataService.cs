using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Entities
{
    public class DataService
    {
        [Key]
        public long Id { get; set; }
        public int FacilityId { get; set; }
        public string? ProviderName { get; set; }
        public string? AccountNumber { get; set; }
        public string? LocalIP { get; set; }
        public int ServicesId { get; set; }
        public string? SupportNumber { get; set; }
        public string? ContractDocument { get; set; }
        public DateTime ContractEndDate { get; set; }
        public bool ContractStatus { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string? Createby { get; set; }
        public DateTime UpdateDate { get; set; }
        public string? UpdateBy { get; set; }
        public bool IsActive { get; set; }
    }
}
