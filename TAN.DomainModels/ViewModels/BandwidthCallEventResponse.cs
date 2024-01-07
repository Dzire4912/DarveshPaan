using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.ViewModels
{
    public class BandwidthCallEventResponse
    {
        public string? AccountName { get; set; }
        public string? AccountId { get; set; }
        public string? SubAccountId { get; set; }
        public string? SubAccountName { get; set; }
        public string? CallId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string? Duration { get; set; }
        public string? CallingNumber { get; set; }
        public string? CalledNumber { get; set; }
        public string? CallType { get; set; }
        public string? CallDirection { get; set; }
        public string? CallResult { get; set; }
        public string? Cost { get; set; }
        public DateTime CreateDate { get; set; }
        public string? HangUpSource { get ; set; }
        public int? SipResponseCode { get; set; }
        public string? SipResponseDescription { get; set; }
        public string? AttestationIndication { get; set; }
        public int? PostDialDelay { get; set; }
        public int? PocketsSent { get; set; }
        public int? PocketsReceived { get; set; }
        public bool? IsActive { get; set; }
        public string? OrganizationName { get; set; }

    }
}
