using labs_lntu_web.DbContexts;
using labs_lntu_web.Models;
using Microsoft.AspNetCore.Mvc;

namespace labs_lntu_web.Controllers.API {
    public class ItemsController : ApiController {
        private readonly ApplicationDbContext dbContext;

        public ItemsController(ApplicationDbContext dbContext) {
            this.dbContext = dbContext;
        }
        [HttpGet("[action]")]
        public IActionResult Get() {
            var items = dbContext.Items.ToList();
            return Ok(items);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> Create() {
            var item = new Item() {
                Name = $"New Item {Guid.NewGuid()}"
            };
            dbContext.Items.Add(item);
            await dbContext.SaveChangesAsync();
            return Ok(item);
        }
    }
}
