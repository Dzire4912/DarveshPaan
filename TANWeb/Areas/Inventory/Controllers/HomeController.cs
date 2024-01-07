using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

namespace TANWeb.Areas.Inventory.Controllers
{
    [Area("Inventory")]
    [ExcludeFromCodeCoverage]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            //HttpContext.Session.SetString("SelectedApp", "Inventory");
            return View();
        }
    }
}
