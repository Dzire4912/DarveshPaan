// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using TAN.DomainModels.Entities;
using TAN.Repository.Abstractions;

namespace TANWeb.Areas.Identity.Pages.Account
{
    [ExcludeFromCodeCoverage]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<AspNetUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly IUnitOfWork _uow;

        public LogoutModel(SignInManager<AspNetUser> signInManager,IUnitOfWork unitOfWork, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
            _uow = unitOfWork;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var log=_uow.loginAuditRepo.GetAll().Where(x=>x.UserId== userId ).OrderByDescending(x=>x.LoginTime).FirstOrDefault();

            if (log !=null)
            { 
                LoginAudit logout=new LoginAudit();
                logout.LoginAuditId = log.LoginAuditId;
                logout.UserId=userId;
              //  logout.LoginStatus = false;
                logout.LogOutTime= DateTime.UtcNow;
                _uow.loginAuditRepo.Update(logout);
                _uow.SaveChanges();
            }
            _logger.LogInformation("User logged out.");
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // This needs to be a redirect so that the browser performs a new
                // request and the identity for the user gets updated.
                return RedirectToPage();
            }
        }
    }
}
