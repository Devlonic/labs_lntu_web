using labs_lntu_web.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace labs_lntu_web.Controllers
{
    public class HostsController : Controller {
        public IActionResult Index() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { });
        }
    }
}
