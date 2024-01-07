using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.ViewModels
{
    public class ManageUserRolesViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<roleItemlist> RoleItems { get; set; }
        public List<appItemlist> AppItems { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public string ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public IList<UserRolesViewModel> UserRoles { get; set; }
        public List<userTypeItemList> UserTypesItems { get; set; }
    }

    public class UserRolesViewModel
    {
        public string RoleName { get; set; }
        public string RoleId { get; set; }
        public string AppId { get; set; }
        public bool Selected { get; set; }
        public string AppName { get; set; }
    }
    public class PermissionViewModel
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public int ModuleId { get; set; }
        public int AppId { get; set; }
        public IList<Itemlist> ModuleList { get; set; }
        public IList<appItemlist> AppList { get; set; }
        public IList<roleItemlist> RoleList { get; set; }
        public IList<RoleClaimsViewModel> RoleClaims { get; set; }
    }

    public class RoleClaimsViewModel
    {

        public string Type { get; set; }
        public string Value { get; set; }
        public bool Selected { get; set; }
        public int ModuleId { get; set; }
        public int ApplicationId { get; set; }
    }
    public class Itemlist
    {
        public string Text { get; set; }
        public int Value { get; set; }
    }
    public class appItemlist
    {
        public string Text { get; set; }
        public int Value { get; set; }
        public bool Selected { get; set; }
    }
    public class roleItemlist
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }
    public class facilityItemList
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public bool Selected { get; set; }
    }
    public class userTypeItemList
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }
}
