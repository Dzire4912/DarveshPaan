using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TAN.DomainModels.Entities;
using System.ComponentModel.DataAnnotations;
using TANWeb.Resources;
using TAN.Repository.Abstractions;
using Microsoft.AspNetCore.DataProtection;
using Serilog;
using TANWeb.Helpers;
using TANWeb.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace TANWeb.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    [ExcludeFromCodeCoverage]
    public class OtpVerificationModel : PageModel
    {
        private readonly UserManager<AspNetUser> _userManager;
        private readonly ILogger<OtpVerificationModel> _logger;
        private readonly SignInManager<AspNetUser> _signInManager;
        private IConfiguration _configuration;
        private IUnitOfWork _uow;
        private readonly IDataProtector _protector;
        private Utils utils = new Utils();
        public OtpVerificationModel(UserManager<AspNetUser> userManager, ILogger<OtpVerificationModel> logger, SignInManager<AspNetUser> signInManager, IConfiguration configuration, IUnitOfWork uow, IDataProtectionProvider dataProvider)
        {
            _userManager = userManager;
            _logger = logger;
            _signInManager = signInManager;
            _configuration = configuration;
            _uow = uow;
            _protector = dataProvider.CreateProtector("Code");
        }

        [BindProperty]
        public InputModel Input { get; set; }


        public class InputModel
        {
            public string? Email { get; set; }
            [Required(ErrorMessage = "Please enter 6 digits OTP")]
            [Display(Name = "OTP")]
            public string EmailOTP { get; set; }
            [Display(Name = "Do not ask on this computer")]
            public bool RememberMe { get; set; }
        }
        public async Task<IActionResult> OnGet(string Email)
        {
            try
            {
                string decodedEmail = Convert.ToString(_protector.Unprotect(Email));
                var _user = await _userManager.FindByEmailAsync(decodedEmail);
                if (_user != null)
                {
                    Input = new InputModel
                    {
                        Email = decodedEmail
                    };
                    return Page();
                }
            }
            catch (Exception ex)
            {
                Log.Error("OTP verification error ", ex.Message);
            }
            return RedirectToPage("./Login");
        }


        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                string returnUrl = Url.Content("~/");
                if(string.IsNullOrEmpty(Input.EmailOTP) || Input.EmailOTP.Length < 6)
                {
                    ModelState.AddModelError(string.Empty, "Please enter 6 digits OTP");
                    return Page();
                }
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByEmailAsync(Input.Email);
                    if (user != null)
                    {
                        var result = await _signInManager.TwoFactorSignInAsync("Email", Input.EmailOTP, true, Input.RememberMe);
                        if (result.Succeeded)
                        {
                            if (!user.EmailConfirmed)
                            {
                                user.EmailConfirmed = true;
                                HttpContext.Session.Remove("EmailOTP");
                                await _userManager.UpdateAsync(user);
                            }

                            if (user.IsUsingTempPassword == 0)
                            {
                                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                                return RedirectToPage("./UpdatePassword", new { code = code });
                            }

                            await _userManager.UpdateSecurityStampAsync(user);
                            var defApp = _uow.ApplicationRepo.GetAll().Where(x => x.ApplicationId == (user.DefaultAppId)).FirstOrDefault();
                            var appName = _configuration[TANResource.PBJSnap];
                            if (defApp.Name == appName)
                            {
                                returnUrl = TANResource.PBJDashboardPath;
                            }
                            else if (defApp.Name == TANResource.Inventory)
                            {
                                returnUrl = TANResource.InventoryDashboardPath;
                            }
                            return LocalRedirect(returnUrl);
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid OTP.");
                        }
                        return Page();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("OTP verification post err ", ex.Message);
            }
            finally
            {
                _userManager.Dispose();
            }
            return RedirectToPage("./OtpVerification", new { Email = _protector.Protect(Input.Email) });
        }
    }
}
