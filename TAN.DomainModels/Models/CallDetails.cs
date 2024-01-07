using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;

namespace TAN.DomainModels.Models
{
    public class CallDetails
    {
        public List<Link> Links { get; set; }
        public Data Data { get; set; }
        public List<Errors> Errors { get; set; }
    }

    public class Link
    {
        public string? Href { get; set; }
        public string? Rel { get; set; }
    }

    public class Data
    {
        public int TotalCount { get; set; }
        public List<BandwidthCallEvents>? Calls { get; set; }
    }

    public class Errors
    {
        [JsonProperty("description")]
        public string? Description { get; set; }
    }

    public class CallsId
    {
        public string? CallId { get; set; }
    }

    public class ExportCallHistoryRequest
    {
        public string? OrganizationName { get; set; }
        public string? CallDirection { get; set; }
        public string? CallDetails { get; set; }
        public string? CallType { get; set; }
        public string? CallResult { get; set; }
        public string? HangUpSource { get; set; }
        public string? SearchValue { get; set; }
        public string? FileName { get; set; }
    }

}
