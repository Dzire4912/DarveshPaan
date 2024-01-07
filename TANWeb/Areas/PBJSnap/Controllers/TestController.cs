using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TANWeb.Areas.PBJSnap.Controllers
{
    [Area("PBJSnap")]
    [AllowAnonymous]
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
