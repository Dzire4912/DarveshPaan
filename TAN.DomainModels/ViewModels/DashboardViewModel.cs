using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;

namespace TAN.DomainModels.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class DashboardViewModel
    {
        public List<Blog> blogs { get; set; }
        public List<DashboardFacilityViewModel> facilities { get; set; }
        public List<OrganizationFacilityModel> organizationFacilities { get; set; }
        public List<Employee> employees { get; set; }
    }
    public class Blog
    {
        public string Title { get; set; }
        public string date { get; set; }
        public string Description { get; set; }
    }
    public class DashboardFacilityViewModel
    {
        public int Id { get; set; }
        public string FacilityID { get; set; }
        public string FacilityName { get; set; }
        public int Exempt { get; set; } = 0;
        public int NonExempt { get; set; } = 0;
        public int Contract { get; set; } = 0;
        public int Agency { get; set; } = 0;
        public int Employees { get; set; } = 0;
    }
}
