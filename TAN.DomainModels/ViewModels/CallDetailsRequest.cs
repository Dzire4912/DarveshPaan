using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Helpers;

namespace TAN.DomainModels.ViewModels
{
    public class CallDetailsRequest
    {
        public string? LocationName { get; set; }
        public string? HangUpSource { get; set; }
        public string? CallDirection { get; set; }
        public string? CallResult { get; set; }
        public string? CallType { get; set; }
        public string? SearchValue { get; set; }
        public string? SortOrder { get; set; }
        public int PageNo { get; set; }
        public int PageSize { get; set; }
        public string? searchvalue { get; set; } 
        string? start { get; set; }
        string? length { get; set; }
        int draw { get; set; }
        string? sortColumn { get; set; }
        string? sortDirection { get; set; }
    }
}
