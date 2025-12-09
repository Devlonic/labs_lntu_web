using labs_lntu_web.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace labs_lntu_web.Controllers.API {
    public class HealthController : ApiController {
        private readonly PingWorker pingWorker;

        public HealthController(PingWorker pingWorker) {
            this.pingWorker = pingWorker;
        }

        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult GetHealth() {
            return pingWorker.ServiceIsRunning ? Ok("ok") : Ok("pinger is not running");
        }
    }
}
