using System.Net.NetworkInformation;

namespace labs_lntu_web.Services {
    public interface IPinger : IDisposable {
        Task<PingReply> SendPingAsync(string hostNameOrAddress, int timeout);
    }
}
