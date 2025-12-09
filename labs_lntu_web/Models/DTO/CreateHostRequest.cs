using FluentValidation;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace labs_lntu_web.Models.DTO {
    public class ModifyHostRequest {
        public string RemoteAddress { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;
        public string OperatingSystem { get; set; } = string.Empty;
        public string Interface { get; set; } = string.Empty;
        public bool? Enabled { get; set; }

        public class ModifyHostRequestValidator : AbstractValidator<ModifyHostRequest> {
            public ModifyHostRequestValidator() {
                RuleFor(x => x.RemoteAddress)
                    .NotEmpty().WithMessage("RemoteAddress is required.")
                    .MaximumLength(255).WithMessage("RemoteAddress cannot exceed 255 characters.")
                    .Must(BeAValidIpOrHostname).WithMessage("RemoteAddress must be a valid IP address or hostname.");
                RuleFor(x => x.HostName)
                    .NotEmpty().WithMessage("HostName is required.")
                    .MaximumLength(100).WithMessage("HostName cannot exceed 100 characters.");
                RuleFor(x => x.Enabled)
                    .NotNull().WithMessage("Enabled is required.");
                RuleFor(x => x.OperatingSystem)
                    .MaximumLength(100).WithMessage("OperatingSystem cannot exceed 100 characters.");
                RuleFor(x => x.Interface)
                    .MaximumLength(100).WithMessage("Interface cannot exceed 100 characters.");
            }

            private bool BeAValidIpOrHostname(string remoteAddress) {
                if ( IPAddress.TryParse(remoteAddress, out _) )
                    return true;

                var hostType = Uri.CheckHostName(remoteAddress);

                return hostType == UriHostNameType.Dns ||
                       hostType == UriHostNameType.IPv4 ||
                       hostType == UriHostNameType.IPv6;
            }
        }
    }
}
