using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TAN.DomainModels.Entities;

namespace TAN.DomainModels.ViewModels
{
    public class KronosFormViewModel
    {

        //blob
        [Required(ErrorMessage = "StorageConnectionString is Required")]
        public string StorageConnectionString { get; set; }

        [Required(ErrorMessage = "KronosDecryptOrEncryptKeyBlobNameGLC is Required")]
        public string KronosDecryptOrEncryptKeyBlobNameGLC { get; set; }

        [Required(ErrorMessage = "KronosDecryptionPasswordGLC is Required")]
        [DataType(DataType.Password)]
        public string KronosDecryptionPasswordGLC { get; set; }
        //sftp
       
        [Required(ErrorMessage = "KronosUserNameGLC is Required")]
        public string KronosUserNameGLC { get; set; }

        [Required(ErrorMessage = "KronosPasswordGLC is Required")]
        [DataType(DataType.Password)]
        public string KronosPasswordGLC { get; set; }

        [Required(ErrorMessage = "KronosHostGLC is Required")]
        public string KronosHostGLC { get; set; }

        [Required(ErrorMessage = "KronosPortGLC is Required")]
        public string KronosPortGLC { get; set; }
        //file

        [Required(ErrorMessage = "KronosPunchExportGLC is Required")]
        public string KronosPunchExportGLC { get; set; }

        [Required(ErrorMessage = "KronosPayrollGLC is Required")]
        public string KronosPayrollGLC { get; set; }
        //org
        [Required(ErrorMessage = "OrganizationId is Required")]       
        public string OrganizationId { get; set; }

        public List<Itemlist> OrgList { get; set; }

        //table
        public List<KronosOrgVM> KronosOrgList {  get; set; }    
    }
    public class KronosOrgVM
    {
        public int OrgId { get; set; }
        public string OrgName { get; set; }
        public int FacilityCount {  get; set; }
        public bool IsActive {  get; set; }

    }
}
