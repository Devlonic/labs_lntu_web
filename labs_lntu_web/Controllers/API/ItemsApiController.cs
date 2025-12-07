using labs_lntu_web.Models;
using Microsoft.AspNetCore.Mvc;

namespace labs_lntu_web.Controllers.API {
    [Route("api/[controller]")]
    public class ItemsController : ApiController {
        [HttpGet("[action]")]
        public IActionResult Get() {
            using var dbContext = new DbContexts.ApplicationDbContext();
            var items = dbContext.Items.ToList();
            return Ok(items);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> Create() {
            using var dbContext = new DbContexts.ApplicationDbContext();
            var item = new Item() {
                Name = $"New Item {Guid.NewGuid()}"
            };
            dbContext.Items.Add(item);
            await dbContext.SaveChangesAsync();
            return Ok(item);
        }
    }
}
