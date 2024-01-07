using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace TANWeb.Areas.TANUserManagement.Controllers
{
    [Area("PBJSnap")]
    [AllowAnonymous]
    [ExcludeFromCodeCoverage]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
