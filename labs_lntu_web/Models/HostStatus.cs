namespace labs_lntu_web.Models {
    public class HostStatus {
        public int? HostId { get; set; }
        public Host? Host { get; set; }
        public bool? IsOnline { get; set; }
        public DateTime CheckedAt { get; set; }
        public int TotalSuccess { get; set; }
        public int TotalFailure { get; set; }
        public int TotalRequests { get; set; }
        public ulong AverageTimeMilisec { get; set; }
    }
}
