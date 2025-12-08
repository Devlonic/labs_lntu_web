using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace labs_lntu_web.Controllers.API {
    public class HostsController : ApiController {
        [HttpGet("/")]
        public async Task<IActionResult> GetAll() {
            using var dbContext = new DbContexts.ApplicationDbContext();
            var hosts = await dbContext.Hosts.ToListAsync();
            return Ok(hosts);
        }
        [HttpGet("/{id}")]
        public async Task<IActionResult> GetById(int id) {
            using var dbContext = new DbContexts.ApplicationDbContext();
            var host = dbContext.Hosts.Find(id);
            return Ok();
        }
        [HttpPost("/")]
        public async Task<IActionResult> AddHost() {
            using var dbContext = new DbContexts.ApplicationDbContext();
            var hosts = await dbContext.Hosts.ToListAsync();
            return Ok(hosts);
        }
    }
}
