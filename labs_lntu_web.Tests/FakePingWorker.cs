using labs_lntu_web.Services;
using labs_lntu_web.Tasks;
using Microsoft.Extensions.Logging;

namespace labs_lntu_web.Tests {
    public class FakePingWorker : PingWorker {
        public FakePingWorker(ILogger<PingWorker> logger, IServiceProvider sp, HostsResultsTempStorage cache) : base(logger, sp, cache) {
        }


        public override async Task<bool> RestartService() {
            return true;
        }
    }
}
