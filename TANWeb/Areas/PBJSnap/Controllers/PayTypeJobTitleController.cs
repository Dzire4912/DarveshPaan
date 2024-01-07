using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using TAN.DomainModels.Entities;
using TAN.Repository.Abstractions;

namespace TANWeb.Areas.PBJSnap.Controllers
{
    [ExcludeFromCodeCoverage]
    [Area("PBJSnap")]
    [Authorize]
    public class PayTypeJobTitleController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly Microsoft.AspNetCore.Identity.UserManager<AspNetUser> _UserManager;
        public PayTypeJobTitleController(IUnitOfWork uow, Microsoft.AspNetCore.Identity.UserManager<AspNetUser> UserManager)
        {
            _uow = uow;
            _UserManager = UserManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetPayCode()
        {
            try
            {
                return Json(await _uow.PayTypeCodesRepo.GetAllPayTypeCodes());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetPayCode :{ErrorMsg}", ex.Message);
            }
            return NotFound();
        }

        [HttpGet]
        public IActionResult GetJobTitle()
        {
            try
            {
                return Json(_uow.JobCodesRepo.GetAll().Where(x => x.IsActive).ToList());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetJobTitle :{ErrorMsg}", ex.Message);
            }
            return NotFound();
        }
    }
}
