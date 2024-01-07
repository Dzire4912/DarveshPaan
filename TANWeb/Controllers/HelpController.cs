using Microsoft.AspNetCore.Mvc;

namespace TANWeb.Controllers
{
    public class HelpController : Controller
    {
        private readonly IConfiguration _configuration;

        public HelpController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [Route("/help")]
        public IActionResult Index()
        {
            var server = _configuration.GetValue<string>("UserManualPath:Server");
            var filePath = _configuration.GetValue<string>("UserManualPath:FilePath");
            var fileName = _configuration.GetValue<string>("UserManualPath:FileName");
            ViewBag.FilePath = server + filePath + fileName + ".pdf";
            return View();
        }
    }
}
