using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Entities
{
    public class KronosBlobStorageCreds
    {
        [Key]
        public int Id {  get; set; }

        [Required(ErrorMessage = "StorageConnectionString is Required")]

        public string StorageConnectionString { get; set; }

        [Required(ErrorMessage = "KronosDecryptOrEncryptKeyBlobNameGLC is Required")]

        public string KronosDecryptOrEncryptKeyBlobNameGLC { get; set; }

        [Required(ErrorMessage = "KronosDecryptionPasswordGLC is Required")]

        public string KronosDecryptionPasswordGLC { get; set; }
        public OrganizationModel OrganizationModel { get; set; }
        [ForeignKey("OrganizationModel")]
        public int OrganizationId { get; set; }
        public bool IsActive { get; set; } = true;

    }
}
