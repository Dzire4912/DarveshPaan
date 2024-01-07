using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Entities
{
    public class UploadClientFileDetails
    {
        [Key]
        public int Id { get; set; }
        public int? OrganizationId { get; set; }
        public int? FacilityId { get; set; }
        public string? CarrierName { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool? IsActive { get; set; }
        public int IsProcessed { get; set; }
    }
}
