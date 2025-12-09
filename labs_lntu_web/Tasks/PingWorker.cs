
using labs_lntu_web.Models;
using labs_lntu_web.Models.DTO;
using labs_lntu_web.Services;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace labs_lntu_web.Tasks {
    public interface IPingWorker {
        Task PingHostsAsync(IEnumerable<HostData> hosts, int maxParallel, CancellationToken ct = default);
        Task<bool> RestartService();
    }
    public class PingWorker : BackgroundService, IPingWorker {
        private readonly ILogger<PingWorker> _logger;
        private readonly IServiceProvider _sp;
        private readonly HostsResultsTempStorage cache;
        private CancellationTokenSource? cts;
        public bool ServiceIsRunning { get; private set; } = false;
        public PingWorker(ILogger<PingWorker> logger, IServiceProvider sp, HostsResultsTempStorage cache) {
            _logger = logger;
            _sp = sp;
            this.cache = cache;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            while ( !stoppingToken.IsCancellationRequested ) {
                cts = new CancellationTokenSource();
                try {
                    _logger.LogInformation("PingWorker running at: {time}", DateTimeOffset.Now);
                    using var scope = _sp.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<DbContexts.ApplicationDbContext>();
                    var hosts = dbContext.Hosts.ToList();
                    //var hosts = new List<HostData>() {
                    //new HostData() {
                    //    Id = 1,
                    //    RemoteAddress = "1.1.1.1",
                    //    Enabled = true,
                    //    Name = "Cloudflare DNS",
                    //    },
                    //new HostData() {
                    //    Id = 2,
                    //    RemoteAddress = "google.com",
                    //    Enabled = true,
                    //    Name = "Google",
                    //    },
                    //};
                    if ( hosts.Count == 0 ) {
                        _logger.LogInformation("No hosts to ping. Waiting 10 sec...");
                        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    }
                    ServiceIsRunning = true;
                    await PingHostsAsync(hosts, 20, cts.Token);
                    ServiceIsRunning = false;
                    _logger.LogInformation("PingWorker service is restarting...");
                }
                catch ( Exception ex ) {
                    _logger.LogError(ex, "Error in PingWorker: {Message}", ex.Message);
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }

        }

        public async Task PingHostsAsync(IEnumerable<HostData> hosts, int maxParallel, CancellationToken ct = default) {
            var tasks = new List<Task>();
            var timeout = 2000;
            foreach ( var host in hosts ) {
                var task = Task.Run(async () => {
                    while ( !ct.IsCancellationRequested ) {
                        try {
                            using var scope = _sp.CreateScope();
                            using var ping = scope.ServiceProvider.GetRequiredService<IPinger>();
                            var reply = await ping.SendPingAsync(host.RemoteAddress, timeout);
                            UpdateHostStatus(host, reply, null);
                            cache.UpdateHost(host);
                            _logger.LogInformation("Pinged {Host} - Status: {Status}, RTT: {RTT}ms", host.RemoteAddress, reply.Status, reply.RoundtripTime);
                            await Task.Delay(TimeSpan.FromSeconds(1), ct);
                        }
                        catch ( Exception e ) {
                            UpdateHostStatus(host, null, e);
                            _logger.LogWarning("Error pinging {Host}: {Message}", host.RemoteAddress, $"{e.Message}: {e.InnerException?.Message}");
                            cache.UpdateHost(host);
                        }
                    }
                    _logger.LogInformation("Stopping ping task for host {Host}", host.RemoteAddress);
                }, ct);

                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }
        private void UpdateHostStatus(HostData host, PingReply? reply, Exception? exception) {
            host.TotalSuccess += reply?.Status == IPStatus.Success ? 1 : 0;
            host.TotalFailure += reply?.Status != IPStatus.Success ? 1 : 0;
            host.SequenceOfFailures = reply?.Status != IPStatus.Success ? host.SequenceOfFailures + 1 : 0;
            host.CheckedAt = DateTime.UtcNow;
            host.TotalRequests += 1;
            host.HostStatus = GetHostStatus(host, reply);
            if ( host.LatestIpStatuses.Count > 6 )
                host.LatestIpStatuses.Clear();
            host.LatestIpStatuses.Add(reply?.Status ?? IPStatus.Unknown);
            host.RoundTripTimeMs = reply?.Status == IPStatus.Success ? reply.RoundtripTime : -1;
            if ( exception != null )
                host.LatestExceptionMsg = $"{exception.Message}: {exception.InnerException?.Message}";
        }
        public virtual HostStatus GetHostStatus(HostData host, PingReply? reply) {
            if ( !host.Enabled )
                return HostStatus.PingHalted;
            if ( reply == null )
                return HostStatus.Error;
            if ( host.SequenceOfFailures > 0 && host.SequenceOfFailures < 5 )
                return HostStatus.Delayed;
            return reply?.Status == IPStatus.Success ? HostStatus.Online : HostStatus.Offline;
        }
        public async Task<bool> RestartService() {
            if(cts == null)
                return false;
            await cts.CancelAsync();
            _logger.LogInformation("PingWorker service restart requested.");
            return true;
        }
    }
}
