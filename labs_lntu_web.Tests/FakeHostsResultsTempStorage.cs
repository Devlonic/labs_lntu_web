using labs_lntu_web.Models;
using labs_lntu_web.Services;
using System.Collections.Concurrent;

namespace labs_lntu_web.Tests {
    public class FakeHostsResultsTempStorage : HostsResultsTempStorage {
        public  ConcurrentDictionary<int, HostData> _results = new();
        public override void Init(IEnumerable<HostData> hosts) {
            _results.Clear();
            foreach ( var host in hosts ) {
                _results.TryAdd(host.Id, host);
            }
        }
        public override IEnumerable<HostData> GetAll() {
            return _results.Values;
        }
        public override void UpdateHost(HostData host) {
            _results.AddOrUpdate(host.Id, host, (key, oldValue) => host);
        }
    }
}
