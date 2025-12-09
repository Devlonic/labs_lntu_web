using labs_lntu_web.DbContexts;
using labs_lntu_web.Models;
using labs_lntu_web.Models.DTO;
using labs_lntu_web.Services;
using labs_lntu_web.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace labs_lntu_web.Controllers.API {
    /// <summary>
    /// Manages CRUD operations for monitored hosts.
    /// </summary>
    public class HostsController : ApiController {
        private readonly ApplicationDbContext dbContext;
        private readonly PingWorker pingWorker;
        private readonly ILogger<HostsController> logger;
        private readonly HostsResultsTempStorage hostsCacheStorage;

        public HostsController(ApplicationDbContext dbContext, PingWorker pingWorker, ILogger<HostsController> logger, HostsResultsTempStorage hostsCacheStorage) {
            this.dbContext = dbContext;
            this.pingWorker = pingWorker;
            this.logger = logger;
            this.hostsCacheStorage = hostsCacheStorage;
        }
        /// <summary>
        /// Returns a list of all registered hosts.
        /// </summary>
        /// <remarks>
        /// Possible status codes:
        /// * 200 - Hosts returned successfully
        /// * 500 - Internal server error
        /// </remarks>
        /// <returns>List of hosts.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<HostData>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll() {
            var hosts = await dbContext.Hosts.AsNoTracking().ToListAsync();
            return Ok(hosts);
        }

        /// <summary>
        /// Retrieves a host by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the host.</param>
        /// <remarks>
        /// Possible status codes:
        /// * 200 - Host found and returned
        /// * 404 - Host not found
        /// * 500 - Internal server error
        /// </remarks>
        /// <returns>Host object.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(HostData), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id) {
            var host = await dbContext.Hosts.FindAsync(id);
            if ( host == null )
                return CustomNotFound(id);

            return Ok(host);
        }

        /// <summary>
        /// Creates a new host entry.
        /// </summary>
        /// <param name="request">Host configuration model.</param>
        /// <remarks>
        /// After creation, the ping background worker is restarted.
        ///
        /// Possible status codes:
        /// * 200 - Host successfully created
        /// * 400 - Invalid request model
        /// * 500 - Internal server error
        /// </remarks>
        /// <returns>Created host object.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(HostData), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddHost([FromBody] ModifyHostRequest request) {
            var host = new HostData() {
                Name = request.HostName,
                RemoteAddress = request.RemoteAddress,
                Enabled = request.Enabled ?? false,
            };

            dbContext.Hosts.Add(host);
            await dbContext.SaveChangesAsync();

            _ = pingWorker.RestartService();

            return Ok(host);
        }

        /// <summary>
        /// Updates an existing host record.
        /// </summary>
        /// <param name="id">Host identifier.</param>
        /// <param name="request">Updated host configuration.</param>
        /// <remarks>
        /// Possible status codes:
        /// * 200 - Host updated successfully
        /// * 404 - Host was not found
        /// * 400 - Invalid model
        /// * 500 - Internal server error
        /// </remarks>
        /// <returns>Updated host object.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(HostData), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateHost(int id, [FromBody] ModifyHostRequest request) {
            var existingHost = await dbContext.Hosts.FindAsync(id);
            if ( existingHost == null )
                return CustomNotFound(id);

            existingHost.Name = request.HostName;
            existingHost.RemoteAddress = request.RemoteAddress;
            existingHost.Enabled = request.Enabled ?? false;

            await dbContext.SaveChangesAsync();

            _ = pingWorker.RestartService();

            return Ok(existingHost);
        }

        /// <summary>
        /// Deletes a host
        /// </summary>
        /// <param name="id">The ID of the host to delete.</param>
        /// <remarks>
        /// Possible status codes:
        /// * 200 - Host successfully deleted
        /// * 404 - Host not found
        /// * 500 - Internal server error
        /// </remarks>
        /// <returns>Deleted host object.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(HostData), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteHost(int id) {
            var existingHost = await dbContext.Hosts.FindAsync(id);
            if ( existingHost == null )
                return CustomNotFound(id);

            dbContext.Hosts.Remove(existingHost);
            await dbContext.SaveChangesAsync();

            _ = pingWorker.RestartService();

            return Ok(existingHost);
        }

        /// <summary>
        /// Returns the latest ping statuses from the monitoring cache
        /// </summary>
        /// <remarks>
        /// Possible status codes:
        /// * 200 - Status data returned successfully
        /// * 500 - Internal server error
        /// </remarks>
        /// <returns>Status collection.</returns>
        [HttpGet("Statuses")]
        [ProducesResponseType(typeof(IEnumerable<HostData>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public IActionResult GetStatuses() {
            var statuses = hostsCacheStorage.GetAll();
            return Ok(statuses);
        }

        [NonAction]
        public virtual IActionResult CustomNotFound(int id) {
            var errorModel = new ErrorViewModel {
                Title = $"The requested resource '{id}' was not found.",
                Status = StatusCodes.Status404NotFound,
                TraceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };
            return NotFound(errorModel);
        }
    }
}
