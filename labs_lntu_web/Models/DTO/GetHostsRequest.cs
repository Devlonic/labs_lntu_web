using System.ComponentModel.DataAnnotations;

namespace labs_lntu_web.Models.DTO {
    public class Get {
    }
    public class CreateHostRequest {
        [Required]
        public string RemoteAddress { get; set; } = string.Empty;
        [Required]
        public string HostName { get; set; } = string.Empty;
        public string OperatingSystem { get; set; } = string.Empty;
        public string Interface { get; set; } = string.Empty;
    }
}
