using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class OrganizationSearchViewModel
    {
        public string SearchByOrganizationId { get; set; }

        public string SearchByOrganizationName { get; set; }
        public string SearchByOrganizationState { get; set; }

    }
}
