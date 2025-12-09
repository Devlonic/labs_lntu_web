using labs_lntu_web.Models;
using labs_lntu_web.Models.DTO;

namespace labs_lntu_web.Services {
    public interface IHostsService {
        Task<IEnumerable<HostData>> GetAllAsync(CancellationToken ct = default);
        Task<HostData?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<HostData> AddHostAsync(ModifyHostRequest request, CancellationToken ct = default);
        Task<HostData?> UpdateHostAsync(int id, ModifyHostRequest request, CancellationToken ct = default);
        Task<HostData?> DeleteHostAsync(int id, CancellationToken ct = default);
        IEnumerable<HostData> GetStatuses();
    }
}
