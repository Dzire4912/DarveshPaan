using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.ViewModels
{
    public class BandWidthCallFilterViewModel
    {
        public string? OrganisationName { get; set; }
        public string? CallDirection { get; set; }
        public string? CallType { get; set; }
        public string? CallResult { get; set;}
        public string? HangUpSource { get; set; }

    }
}
