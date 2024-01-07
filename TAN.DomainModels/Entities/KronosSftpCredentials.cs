using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAN.DomainModels.Entities
{
    public class KronosSftpCredentials
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "KronosUserNameGLC is Required")]
        public string KronosUserNameGLC { get; set; }

        [Required(ErrorMessage = "KronosPasswordGLC is Required")]
        public string KronosPasswordGLC { get; set; }

        [Required(ErrorMessage = "KronosHostGLC is Required")]
        public string KronosHostGLC { get; set; }

        [Required(ErrorMessage = "KronosPortGLC is Required")]
        public string KronosPortGLC { get; set; }
        public OrganizationModel OrganizationModel { get; set; }
        [ForeignKey("OrganizationModel")]
        public int OrganizationId { get; set; }
        public bool IsActive { get; set; } = true;

    }
}
