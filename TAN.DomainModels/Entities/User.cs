using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Entities
{
    public class User
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string UserEmail { get; set; }
        public string Role { get; set; }
        public bool ActiveStatus { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int UserType { get; set; } 
        public int OrgId { get; set; }
        public string RoleId { get; set; }
        public int FacilityId { get;set; }
        public string DefaultAppName { get; set; }
    }
}
