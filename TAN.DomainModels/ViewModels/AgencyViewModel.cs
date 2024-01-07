using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;

namespace TAN.DomainModels.ViewModels
{
    public class AgencyViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="Agency Id is Required")]
        [Display(Name ="Agency Id")]
        [MinLength(5, ErrorMessage = "Agency Id must be atleast of 5 characters!")]
        [MaxLength(16, ErrorMessage = "Agency Id should be upto 16 characters only!")]
        public string? AgencyId { get; set; }
        [Required(ErrorMessage = "Agency Name is Required")]
        [Display(Name = "Agency Name")]
        public string? AgencyName { get; set; }

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
        [MaxLength(5, ErrorMessage = "Zip Code should be of 5 digits only!")]
        [MinLength(5, ErrorMessage = "Zip Code must be at least 5 digits!")]
        public string? ZipCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        //[Display(Name = "Organization Name")]

        //public int OrganizationId { get; set; }

        [Display(Name = "Facility Name")]
        [Required(ErrorMessage = "Facility Name is Required")]
        public List<string> FacilityId { get; set; }

        public string? FacilityName { get;  set; }

        public List<Itemlist>? AgencyIdList { get; set; }
        public List<StateModel>? StateList { get; set; }

        public List<FacilityModel>? FacilityList { get; set; }
        public int FId { get; set; }

    }
}
