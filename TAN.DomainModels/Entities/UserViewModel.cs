using TAN.DomainModels.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Entities
{
    public class UserViewModel
    {
        public List<User> users = new List<User>();
        public List<roleItemlist> RoleList { get; set; }
        public List<Itemlist> OrgList { get; set; }
        public List<Itemlist> FacilityList { get; set; }
        public List<Itemlist> UserTypesList { get; set; }
        public List<Itemlist> ApplicationList { get; set; }
        public List<roleItemlist> PopUpRoles { get; set; }
    }
    public class EditUserViewModel
    {
        public AspNetUser users = new AspNetUser();
        public List<roleItemlist> RoleList { get; set; }
        public List<appItemlist> OrgList { get; set; }
        public List<facilityItemList> FacilityList { get; set; }
        public List<appItemlist> ApplicationList { get; set; }
        public List<appItemlist> UserTypeList { get; set; }
    }
}
