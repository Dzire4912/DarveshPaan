

using TAN.DomainModels.Models;

namespace TAN.DomainModels.ViewModels
{
    public class BandwidthCallSearchViewModel
    {        
            public List<SelectListText>? CallDirection { get; set; }
            public List<SelectListText>? CallType { get; set; }
            public List<SelectListText>? CallResult { get; set; }
            public List<SelectListText>? HangUpSource { get; set; }
            public List<BandwidthAccessList>? bandwidthAccessLists { get; set; }
            public int? UserType { get; set; }
    }

    public class SelectListText
    {
        public string? Text { get; set; }
        public string? Value { get; set; }
    }
}
