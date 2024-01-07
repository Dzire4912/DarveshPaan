using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Entities
{
    public class KronosSftpFileEndpoints
    {
        [Key]
        public int Id {  get; set; }

        [Required(ErrorMessage = "KronosPunchExportGLC is Required")]
        public string KronosPunchExportGLC {  get; set; }

        [Required(ErrorMessage = "KronosPayrollGLC is Required")]
        public string KronosPayrollGLC { get; set; }
        public OrganizationModel OrganizationModel { get; set; }
        [ForeignKey("OrganizationModel")]
        public int OrganizationId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
