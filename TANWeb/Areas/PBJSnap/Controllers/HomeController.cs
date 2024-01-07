using Microsoft.AspNetCore.Mvc;

namespace TANWeb.Areas.PBJSnap.Controllers
{
    [Area("PBJSnap")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
