using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Entities
{
    public class DataServiceType
    {
        [Key]
        public long Id { get; set; }
        public int FacilityId { get; set; }
        public string? ServiceName { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string? Createby { get; set; }
        public DateTime UpdateDate { get; set; }
        public string? UpdateBy { get; set; }
        public bool IsActive { get; set; }
    }
}
