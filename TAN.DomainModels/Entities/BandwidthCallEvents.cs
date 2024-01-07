using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Entities
{
    public class BandwidthCallEvents
    {
        [Key]
        public Int64 Id { get; set; }
        public string? AccountId { get; set; }
        public string? CallId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int? Duration { get; set; }
        public string? CallingNumber { get; set; }
        public string? CalledNumber { get; set; }
        public string? CallDirection { get; set; }
        public string? CallType { get; set; }
        public string? CallResult { get; set; }
        public int SipResponseCode { get; set; }
        public string? SipResponseDescription { get; set; }
        public double? Cost { get; set; }
        public string? SubAccount { get; set; }
        public string? AttestationIndicator { get; set; }
        public string? HangUpSource { get; set; }
        public int? PostDialDelay { get; set; }
        public int? PacketsSent { get; set; } 
        public int? PacketsReceived { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
    }
}
