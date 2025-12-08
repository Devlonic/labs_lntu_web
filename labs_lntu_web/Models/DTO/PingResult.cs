using System.Net.NetworkInformation;

namespace labs_lntu_web.Models.DTO {
    public class PingResult {
        public int HostId { get; set; }
        public string? HostAddress { get; set; }
        public bool? Success { get; set; }
        public IPStatus? StatusCode { get; set; }
        public string? StatusMsg { get; set; }
        public long? RoundtripTimeMs { get; set; }
        public string? InternalExceptionMsg { get; set; }
    }
}