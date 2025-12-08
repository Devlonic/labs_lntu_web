using labs_lntu_web.DbContexts;
using labs_lntu_web.Models;
using labs_lntu_web.Models.DTO;
using labs_lntu_web.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace labs_lntu_web.Controllers.API {
    public class HostsController : ApiController {
        private readonly ApplicationDbContext dbContext;
        private readonly PingWorker pingWorker;
        private readonly ILogger<HostsController> logger;

        public HostsController(ApplicationDbContext dbContext, PingWorker pingWorker, ILogger<HostsController> logger) {
            this.dbContext = dbContext;
            this.pingWorker = pingWorker;
            this.logger = logger;
        }
        [HttpGet("get")]
        public async Task<IActionResult> GetAll() {
            var hosts = await dbContext.Hosts.ToListAsync();
            return Ok(hosts);
        }
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id) {
            var host = dbContext.Hosts.Find(id);
            return Ok();
        }
        [HttpPost("create/")]
        public async Task<IActionResult> AddHost([FromBody]CreateHostRequest request) {
            var host = new HostData() {
                Name = request.HostName,
                RemoteAddress = request.RemoteAddress,
                Enabled = request.Enabled,
            };
            dbContext.Hosts.Add(host);
            await dbContext.SaveChangesAsync();
            var restarted = await pingWorker.RestartService();
            if( !restarted )
                logger.LogWarning("PingWorker service was not restarted after adding a new host.");
            else
                logger.LogInformation("PingWorker service was restarted after adding a new host.");
            return Ok(host);
        }
    }
}
