using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TAN.DomainModels.Helpers.Permissions;

namespace TAN.DomainModels.Models
{
    public class FacilityLocationAccess
    {
        public int OrganizationID { get; set; }
        public string? OrganizationName { get; set; }
        public string? MasterAccountId {get;set;}
        public string? SubAccountId {get;set;}
        public int FacilityID {get;set;}
        public string? FacilityName {get;set;}
        public string? LocationId {get;set;}
    }
}
