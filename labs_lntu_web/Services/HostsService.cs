using FluentValidation;
using labs_lntu_web.DbContexts;
using labs_lntu_web.Models;
using labs_lntu_web.Models.DTO;
using labs_lntu_web.Tasks;
using Microsoft.EntityFrameworkCore;

namespace labs_lntu_web.Services {
    public class HostsService : IHostsService {
        private readonly ApplicationDbContext dbContext;
        private readonly PingWorker pingWorker;
        private readonly HostsResultsTempStorage hostsCacheStorage;
        private readonly IValidator<ModifyHostRequest> modifyHostValidator;

        public HostsService(ApplicationDbContext dbContext, PingWorker pingWorker, HostsResultsTempStorage hostsCacheStorage, IValidator<ModifyHostRequest> modifyHostValidator) {
            this.dbContext = dbContext;
            this.pingWorker = pingWorker;
            this.hostsCacheStorage = hostsCacheStorage;
            this.modifyHostValidator = modifyHostValidator;
        }

        public async Task<IEnumerable<HostData>> GetAllAsync(CancellationToken ct = default) {
            return await dbContext.Hosts
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<HostData?> GetByIdAsync(int id, CancellationToken ct = default) {
            return await dbContext.Hosts.FindAsync(id, ct);
        }

        public async Task<HostData> AddHostAsync(ModifyHostRequest request, CancellationToken ct = default) {
            await modifyHostValidator.ValidateAndThrowAsync(request, ct);

            var host = new HostData {
                Name = request.HostName,
                RemoteAddress = request.RemoteAddress,
                Enabled = request.Enabled ?? false,
            };

            dbContext.Hosts.Add(host);
            await dbContext.SaveChangesAsync(ct);

            _ = pingWorker.RestartService();

            return host;
        }

        public async Task<HostData?> UpdateHostAsync(int id, ModifyHostRequest request, CancellationToken ct = default) {
            await modifyHostValidator.ValidateAndThrowAsync(request, ct);

            var existingHost = await dbContext.Hosts.FindAsync(id, ct);
            if ( existingHost == null )
                return null;

            existingHost.Name = request.HostName;
            existingHost.RemoteAddress = request.RemoteAddress;
            existingHost.Enabled = request.Enabled ?? false;

            await dbContext.SaveChangesAsync(ct);

            _ = pingWorker.RestartService();

            return existingHost;
        }

        public async Task<HostData?> DeleteHostAsync(int id, CancellationToken ct = default) {
            var existingHost = await dbContext.Hosts.FindAsync([id], ct);
            if ( existingHost == null )
                return null;

            dbContext.Hosts.Remove(existingHost);
            await dbContext.SaveChangesAsync(ct);

            _ = pingWorker.RestartService();

            return existingHost;
        }

        public IEnumerable<HostData> GetStatuses() {
            return hostsCacheStorage.GetAll();
        }
    }
}
