using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.ViewModels
{
    public class CostSummaryCsvRequest
    {
        public string? OrganizationName { get; set; }
        public string? FileName { get; set; }
        public string? CallDirection { get; set; }
        public string? CallType { get; set; }
        public string? CallResult { get; set; }
        public string? SearchedValue { get; set; }
    }
}
