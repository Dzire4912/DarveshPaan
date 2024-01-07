using Microsoft.AspNetCore.Identity;
using TAN.DomainModels.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.ViewModels
{
    public class RolesViewModel
    {
        [Required(ErrorMessage = "Please enter role name")]
        [RegularExpression("[a-zA-Z0-9]*[^!@%~?:#$%^&*()0']*", ErrorMessage = "Invalid Input.")]
        public string RoleName { get; set; }

        [Required(ErrorMessage = "Please enter role name")]
        [RegularExpression("[a-zA-Z0-9]*[^!@%~?:#$%^&*()0']*", ErrorMessage = "Invalid Input.")]
        public string SearchRoleName { get; set; }

        public IList<RoleView> RolesList { get; set; }
        public IList<Itemlist> ApplicationList { get; set; }

        [Required(ErrorMessage = "Please select application name")]
        public string ApplicationName { get; set; }
        public string RoleId { get; set; }
        public List<roleItemlist> AppRoleList { get; set; }
        public List<userTypeItemList> UserTypesItems { get; set; }
    }

    public class RoleView
    {
        public string RoleName { get; set; }
        public string RoleId { get; set; }
        public string ApplicationName { set; get; }
        public string ApplicationId { set; get; }
        public string NormalizedName{get;set;}

    }
}
