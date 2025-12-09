using FluentValidation;
using labs_lntu_web.Models;
using labs_lntu_web.Models.DTO;
using labs_lntu_web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace labs_lntu_web.Controllers.API {

    /// <summary>
    /// Manages CRUD operations for monitored hosts.
    /// </summary>
    public class HostsController : ApiController {
        private readonly IHostsService hostsService;
        private readonly ILogger<HostsController> logger;

        public HostsController(IHostsService hostsService, ILogger<HostsController> logger) {
            this.hostsService = hostsService;
            this.logger = logger;
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
            var hosts = await hostsService.GetAllAsync();
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
            var host = await hostsService.GetByIdAsync(id);
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
            try {
                var host = await hostsService.AddHostAsync(request);
                return Ok(host);
            }
            catch ( ValidationException ex ) {
                return CreateValidationErrorResult(ex);
            }
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
            try {
                var updatedHost = await hostsService.UpdateHostAsync(id, request);
                if ( updatedHost == null )
                    return CustomNotFound(id);

                return Ok(updatedHost);
            }
            catch ( ValidationException ex ) {
                return CreateValidationErrorResult(ex);
            }
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
            var deletedHost = await hostsService.DeleteHostAsync(id);
            if ( deletedHost == null )
                return CustomNotFound(id);

            return Ok(deletedHost);
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
            var statuses = hostsService.GetStatuses();
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

        [NonAction]
        private IActionResult CreateValidationErrorResult(ValidationException ex) {
            var errorModel = new ErrorViewModel {
                Title = "Validation failed.",
                Status = StatusCodes.Status400BadRequest,
                TraceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                //Errors = ex.Errors.Select(e => e.).ToDictionary()
            };

            return BadRequest(errorModel);
        }
    }
}
