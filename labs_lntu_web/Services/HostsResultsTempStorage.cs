using labs_lntu_web.Models;
using labs_lntu_web.Models.DTO;
using System.Collections.Concurrent;

namespace labs_lntu_web.Services {
    public class HostsResultsTempStorage {
        public ConcurrentDictionary<int, HostData> _results = new();
        public void Init(IEnumerable<HostData> hosts) {
            _results.Clear();
            foreach ( var host in hosts ) {
                _results.TryAdd(host.Id, host);
            }
        }
        public IEnumerable<HostData> GetAll() {
            return _results.Values;
        }
        public void UpdateHost(HostData host) {
            _results.AddOrUpdate(host.Id, host, (key, oldValue) => host);
        }

    }
}
