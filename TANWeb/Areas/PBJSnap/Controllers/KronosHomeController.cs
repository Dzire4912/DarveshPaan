using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TAN.DomainModels.Entities;
using TAN.Repository.Abstractions;

namespace TANWeb.Areas.PBJSnap.Controllers
{
    [Area("PBJSnap")]
    public class KronosHomeController : Controller
    {
        private readonly UserManager<AspNetUser> _userManager;
        private readonly RoleManager<AspNetRoles> _roleManager;
        private IUnitOfWork _uow;
        public KronosHomeController(IUnitOfWork uow, UserManager<AspNetUser> userManager, RoleManager<AspNetRoles> roleManager)
        {
            _uow = uow;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        [Route("pbj/KronosHome/index")]
        public IActionResult Index()
        {
            return View("~/Areas/PBJSnap/Views/KronosHome/Index.cshtml");
        }
    }
}
