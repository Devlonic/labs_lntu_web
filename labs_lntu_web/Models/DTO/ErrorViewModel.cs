namespace labs_lntu_web.Models.DTO
{
    public class ErrorViewModel
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Status { get; set; } = int.MaxValue;
        public Dictionary<string, string[]> Errors { get; set; } = new();
        public string TraceId { get; set; } = string.Empty;
    }
}
