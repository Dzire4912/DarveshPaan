using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.ViewModels
{
    public class DataServiceTypeViewModel
    {
        public string? DataServiceTypeId { get; set; }
        public string? DataServiceTypeName { get; set; }
        public bool? IsActive { get; set; }
    }
}
