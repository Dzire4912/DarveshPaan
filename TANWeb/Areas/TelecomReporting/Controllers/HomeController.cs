using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.Repository.Abstractions;
using TAN.Repository.Implementations;

namespace TANWeb.Areas.TelecomReporting.Controllers
{
    [Area("TelecomReporting")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<AspNetUser> _userManager;
        private readonly RoleManager<AspNetRoles> _roleManager;
        private readonly SignInManager<AspNetUser> _signInManager;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(UserManager<AspNetUser> userManager, RoleManager<AspNetRoles> roleManager, SignInManager<AspNetUser> signInManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            HttpContext.Session.SetString("SelectedApp", "Reporting");
            HttpContext.Session.SetString("LastActivity", DateTimeOffset.Now.ToString());
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            var user = await _userManager.GetUserAsync(User);
            await _signInManager.SignOutAsync();
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            var agent = UserAgentHelper.GetBrowserName(userAgent);
            var log = _unitOfWork.loginAuditRepo.GetAll().Where(x => x.UserId == user.Id && x.IPAddress == HttpContext.Connection.RemoteIpAddress?.ToString() && x.LogOutTime == null && x.UserAgent == agent).OrderByDescending(x => x.LoginTime).FirstOrDefault();

            if (log != null)
            {

                log.UserId = user.Id;
                log.LoginStatus = Convert.ToInt32(LoginAuditStatus.Success);
                log.LogOutTime = DateTime.UtcNow;
                log.IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                _unitOfWork.loginAuditRepo.Update(log);
                _unitOfWork.SaveChanges();
            }
            Log.Information("Logout", user.UserName + "logged out successfully");
            return RedirectToAction("Index");
        }
    }
}
