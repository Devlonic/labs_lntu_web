using System.Net.NetworkInformation;

namespace labs_lntu_web.Services {
    public class Pinger : IPinger {
        private readonly Ping ping;
        public Pinger() {
            ping = new Ping();
        }
        public void Dispose() => ping.Dispose();

        public async Task<PingReply> SendPingAsync(string hostNameOrAddress, int timeout) {
            return await ping.SendPingAsync(hostNameOrAddress, timeout);
        }
    }
}
