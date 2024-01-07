using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using TAN.DomainModels.Entities;

namespace TAN.DomainModels.ViewModels
{
    public class FacilityViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Facility Id is Required")]
        [Display(Name = "Facility Id")]
        [MinLength(5, ErrorMessage = "Facility Id must be atleast of 5 characters!")]
        [MaxLength(16, ErrorMessage = "Facility Id should be upto 16 characters only!")]
        public string? FacilityID { get; set; }
        [Required(ErrorMessage = "Facility Name is Required")]
        [Display(Name = "Facility Name")]
        public string? FacilityName { get; set; }

        [Required(ErrorMessage = "Address 1 is Required")]
        [Display(Name = "Address 1")]
        public string? Address1 { get; set; }

        [Display(Name = "Address 2")]
        public string? Address2 { get; set; }
        [Required(ErrorMessage = "City is Required")]
        public string? City { get; set; }
        [Required(ErrorMessage = "State is Required")]
        public string? State { get; set; }

        [Required(ErrorMessage = "Zip Code is Required")]
        [Display(Name = "Zip Code")]
        [MaxLength(5, ErrorMessage = "Zip Code should be of 5 digits only!")]
        [MinLength(5, ErrorMessage = "Zip Code must be at least 5 digits!")]
        public string? ZipCode { get; set; }

        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }

        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }

        public string? UpdatedBy { get; set; }

        [Required(ErrorMessage = "Organization Name is Required")]
        [Display(Name = "Organization Name")]
        public string? OrganizationId { get; set; }

        public string? OrganizationName { get; set; }

        [Required(ErrorMessage = "Application Name is Required")]
        [Display(Name = "Application Name")]
        public List<string>? ApplicationId { get; set; }
        public string? LocationId { get; set; }
        public string? LocationDescription { get; set; }

        public List<Itemlist>? OrganizationList { get; set; }
        public List<Itemlist>? ApplicationList { get; set; }
        public List<StateModel>? StateList { get; set; }

        public string? AppId { get; set; }
        public bool HasDeduction { get; set; }=false;
        public int? RegularDeduction { get; set; } = 30;
        public int? OverTimeDeduction { get; set; } = 60;
        public int UserType { get; set; }


    }
}
