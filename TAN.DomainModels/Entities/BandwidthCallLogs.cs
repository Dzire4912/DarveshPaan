using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Entities
{
    public class BandwidthCallLogs
    {
        [Key]
        public Int64 Id { get; set; }
        public string? URL { get; set; }
        public int ResponseStatus { get; set; }
        public string? Response { get; set; }
        public string? CreateBy { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string? UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public bool IsActive { get; set; }
    }
}
