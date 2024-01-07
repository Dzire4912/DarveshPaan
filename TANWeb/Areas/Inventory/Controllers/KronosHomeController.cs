using Microsoft.AspNetCore.Mvc;

namespace TANWeb.Areas.Inventory.Controllers
{
    [Area("Inventory")]
    public class KronosHomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
