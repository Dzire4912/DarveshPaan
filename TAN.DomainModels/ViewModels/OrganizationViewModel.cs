using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using TAN.DomainModels.Entities;

namespace TAN.DomainModels.ViewModels
{
    public class OrganizationViewModel
    {
        [Key]
        [Display(Name = "Organization ID")]
        public int OrganizationID { get; set; }
        [Required(ErrorMessage = "Organization Name is Required")]
        [Display(Name = "Organization Name")]

        public string? OrganizationName { get; set; }

        [Required(ErrorMessage = "Organization Email is Required")]
        [EmailAddress(ErrorMessage = "The Organization Email field is not a valid e-mail address.")]
        [RegularExpression(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", ErrorMessage = "The Organization Email field is not a valid e-mail address.")]
        [Display(Name = "Organization Email")]

        public string? OrganizationEmail { get; set; }

        [Required(ErrorMessage = "Primary Phone is Required")]
        [MaxLength(10, ErrorMessage = "Primary Phone should be of 10 digits only!")]
        [MinLength(10, ErrorMessage = "Primary Phone must be at least 10 digits!")]
        [Display(Name = "Primary Phone")]
        public string? PrimaryPhone { get; set; }

        [MaxLength(10, ErrorMessage = "Secondary Phone should be of 10 digits only!")]
        [MinLength(10, ErrorMessage = "Secondary Phone must be at least 10 digits!")]
        [Display(Name = "Secondary Phone")]
        public string? SecondaryPhone { get; set; }

        public string? Country { get; set; }

        [Required(ErrorMessage = "Address 1 is Required")]
        public string? Address1 { get; set; }

        public string? Address2 { get; set; }

        [Required(ErrorMessage = "Organization City is Required")]
        public string? City { get; set; }
        [Required(ErrorMessage = "Organization State is Required")]
        public string? State { get; set; }
        [Display(Name = "Zip Code")]

        [Required(ErrorMessage = "Organization Zip Code is Required")]
        [MaxLength(5, ErrorMessage = "Zip Code should be of 5 digits only!")]
        [MinLength(5, ErrorMessage = "Zip Code must be at least 5 digits!")]
        public string? ZipCode { get; set; }

        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }

        public DateTime UpdatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Service Type is Required")]
        public List<string>? ServiceType { get; set; }
        [Required]
        public string? MasterAccountId { get; set; }
        [Required(ErrorMessage = "Sub Account Id is Required")]
        public string? SubAccountId { get; set; }

        public ICollection<FacilityViewModel>? Facilities { get; set; }

        public List<StateModel>? StateList { get; set; }
        public List<ServiceDetails>? serviceDetails { get; set; }

        public string? ServiceTypes { get; set; }

    }

    public class ServiceDetails
    {
        public string? ServiceType { get; set; }
        public string? MasterAccountId { get; set; }
        public string? SubAccountId { get; set; }
    }
}
