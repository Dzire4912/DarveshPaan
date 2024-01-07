using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Entities
{
    public class KronosDataTrack
    {
        [Key]
        public string Id { get; set; }
        public int RecordsCount { get; set; } = 0;
        public string? Description { get; set; }
        public string ToTable { get; set; }
        public int Status { get; set; } = 0;
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set;}
        public int AutoSyncCount { get; set; }       
        public string Source { get; set; }
     

    }
}
