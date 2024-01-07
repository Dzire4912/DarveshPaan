using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Entities
{
    public class KronosFacilities
    {
        [Key]
        public int Id { get; set; }       
        public string FacilityId { get; set; }    
        public int OrganizationId {  get; set; }
        public DateTime Refreshmentdate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public bool IsAvailable { get; set; } = false;
        [NotMapped] 
        public string? OrgName { get; set; }

    }
}
