using labs_lntu_web.Core;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;

namespace labs_lntu_web.Models {
    public class HostData {
        public bool Enabled { get; set; }
        public int Id { get; set; }
        public string? Name { get; set; }
        public string RemoteAddress { get; set; } = null!;
        public string? OperatingSystem { get; set; }
        public string? Interface { get; set; }
        public HostStatus HostStatus { get; set; }
        public DateTime CheckedAt { get; set; }
        public int TotalSuccess { get; set; }
        public int TotalFailure { get; set; }
        public int TotalRequests { get; set; }
        public ulong AverageTimeMilisec { get; set; }
        public long RoundTripTimeMs { get; set; }
        [NotMapped]
        public int SequenceOfFailures { get; set; } = 0;
        [NotMapped]
        public string? HostStatusMessage { get; set; }
        [NotMapped]
        public List<IPStatus> LatestIpStatuses { get; set; } = new();
        [NotMapped]
        public string? LatestExceptionMsg { get; set; }
    }
    public enum HostStatus {
        Unknown, Online, Offline, Delayed, PingHalted, Error
    }
}
