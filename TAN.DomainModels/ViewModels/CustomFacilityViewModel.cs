using TAN.DomainModels.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace TAN.DomainModels.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class CustomFacilityViewModel
    {
        public int Id { get; set; }
        public int FacilityID { get; set; }
        public string? FacilityName { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }

        public string? ZipCode { get; set; }

        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }

        public DateTime UpdatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public int OrganizationId { get; set; }

        public string? OrganizationName { get; set; }

        public bool ? IsActive { get; set; }
    }
}
