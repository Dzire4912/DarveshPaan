// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using AspNetCore.ReCaptcha;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TANWeb.Areas.PBJSnap.PBJSnapServices;
using TANWeb.Helpers;
using TANWeb.Interface;
using TANWeb.Models;
using TANWeb.Resources;

namespace TANWeb.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    [ExcludeFromCodeCoverage]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<AspNetUser> _signInManager;
        private readonly UserManager<AspNetUser> _userManager;
        private IUnitOfWork _uow;
        private IConfiguration _configuration;
        private readonly ILogger<LoginModel> _logger;
        private Utils utils = new Utils();
        private readonly IDataProtector _protector;
        private readonly IMailContentHelper _mailContentHelper;
        private readonly IGoogleCaptcha _googleCaptcha;
        private readonly IConfiguration config;
        private readonly IEmailHelper _emailHelper;

        public LoginModel(SignInManager<AspNetUser> signInManager, UserManager<AspNetUser> userManager, ILogger<LoginModel> logger, IUnitOfWork uow, IConfiguration configuration, IDataProtectionProvider dataProvider, IMailContentHelper mailContentHelper, IGoogleCaptcha googleCaptcha, IConfiguration config, IEmailHelper emailHelper)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _uow = uow;
            _configuration = configuration;
            _protector = dataProvider.CreateProtector("Code");
            _mailContentHelper = mailContentHelper;
            _googleCaptcha = googleCaptcha;
            this.config = config;
            _emailHelper = emailHelper;
        }


        [BindProperty]
        public InputModel Input { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public string ReturnUrl { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }
        public class InputModel
        {
            [Required(ErrorMessage = "Please enter your email id")]
            [EmailAddress]
            public string Email { get; set; }
            [Required(ErrorMessage = "Please enter your password")]
            [DataType(DataType.Password)]
            public string Password { get; set; }
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; } = false;

            [Required(ErrorMessage = "Invalid token")]
            public string Token { get; set; } = "";
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, "");
            }
            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }


        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            List<string> recipients = new List<string>();
            bool isRecaptchaScoreLow = false;
            try
            {
                returnUrl ??= Url.Content("~/");

                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

                if (ModelState.IsValid)
                {
                    if (Input.Token != null && Input.Token != "")
                    {
                        var response = await _googleCaptcha.ValidateToken(Input.Token);
                        if (!response.Success)
                        {
                            ModelState.AddModelError(string.Empty, "Invalid Token.");
                            return Page();
                        }
                        /*else
                        {
                            double ScoreThreshold = config.GetValue<double>("ReCaptcha:ScoreThreshold");
                            if (response.Score < ScoreThreshold)
                            {
                                isRecaptchaScoreLow = true;
                            }
                        }*/
                    }
                    var user = await _userManager.FindByEmailAsync(Input.Email);
                    if (user != null)
                    {
                        if (user.UserType == (int)UserTypes.Other)
                        {
                            if (!await _uow.loginAuditRepo.LoginValidate(user.Email))
                            {
                                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                                return Page();
                            }
                        }
                        if (!user.TwoFactorEnabled)
                        {
                            if (isRecaptchaScoreLow)
                            {
                                user.TwoFactorEnabled = true;
                                await _userManager.UpdateAsync(user);
                            }
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return Page();
                    }
                    var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
                    SendMail sendMail = new();
                    EmailTemplateViewModel emailTemplateViewModel = new();
                    recipients.Add(Input.Email);
                    if (result.Succeeded)
                    {
                        var defApp = _uow.ApplicationRepo.GetAll().Where(x => x.ApplicationId == (user.DefaultAppId)).FirstOrDefault();
                        var pbjsnap = _configuration[TANResource.PBJSnap];
                        var inventory = _configuration[TANResource.Inventory];
                        //LoginAudit
                        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                        var agent = UserAgentHelper.GetBrowserName(userAgent);
                        LoginAudit loginAudit = new LoginAudit();
                        loginAudit.UserId = user.Id;
                        loginAudit.LoginStatus = Convert.ToInt32(LoginAuditStatus.Success);
                        loginAudit.LoginTime = DateTime.UtcNow;
                        loginAudit.IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                        loginAudit.UserAgent = agent;
                        _uow.loginAuditRepo.Add(loginAudit);
                        _uow.SaveChanges();
                        //assign url based on default app

                        if (defApp.Name == inventory)
                        {
                            returnUrl = TANResource.InventoryDashboardPath;
                        }
                        else if (defApp.Name == pbjsnap)
                        {
                            returnUrl = TANResource.PBJDashboardPath;
                        }
                        else
                        {
                            returnUrl= TANResource.ReportingDashboardPath;
                        }
                        if (user != null)
                        {
                            await _userManager.UpdateSecurityStampAsync(user);
                            HttpContext.Session.SetString("UserEmail", Input.Email);

                            if (isRecaptchaScoreLow)
                            {
                                if (await TWOFA())
                                {
                                    return RedirectToPage("./OtpVerification", new { Email = _protector.Protect(Input.Email.ToString()) });
                                }
                                else
                                {
                                    await _signInManager.SignOutAsync();
                                    ModelState.AddModelError(string.Empty, "Invalid login Attempt.");
                                    return Page();
                                }
                            }

                            if (!user.EmailConfirmed)
                            {
                                HttpContext.Session.SetString("EmailOTP", utils.GenerateOTP());
                                sendMail.Recipients = recipients.ToArray();
                                emailTemplateViewModel.Otp = HttpContext.Session.GetString("EmailOTP")?.ToString();
                                emailTemplateViewModel.MailFormat = "OTP";
                                emailTemplateViewModel.TemplateFilePath = TANResource.ConfirmAccountEmailTemplatePath;
                                emailTemplateViewModel.Message = "OTP.";
                                emailTemplateViewModel.UserName = user.FirstName;
                                string server = _configuration.GetValue<string>("DomainPath:Server");
                                string logoPath = _configuration.GetValue<string>("DomainPath:LogoPath");
                                emailTemplateViewModel.LogoPath = server + logoPath + "logo1.png";
                                emailTemplateViewModel.OtpPath = server + logoPath + "otp.png";
                                sendMail.Subject = "Think Anew Email One-Time Password Verification";
                                sendMail.Body = await _mailContentHelper.CreateMailContent(emailTemplateViewModel);

                                bool isMailSent = await _emailHelper.SendEmailAsync(sendMail);
                                if (isMailSent)
                                {
                                    return RedirectToPage("./EmailPhoneConfirmation");
                                }
                                return RedirectToPage("./EmailPhoneConfirmation");
                            }
                            if (user.IsUsingTempPassword == 0)
                            {
                                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                                return RedirectToPage("./UpdatePassword", new { code = code });
                            }
                            Log.Information("User logged in.");

                            return LocalRedirect(returnUrl);
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                            return Page();
                        }

                    }
                    if (result.RequiresTwoFactor)
                    {
                        if (await TWOFA())
                        {
                            return RedirectToPage("./OtpVerification", new { Email = _protector.Protect(Input.Email.ToString()) });
                        }
                        else
                        {
                            await _signInManager.SignOutAsync();
                            ModelState.AddModelError(string.Empty, "Invalid login Attempt.");
                            return Page();
                        }
                    }
                    if (result.IsLockedOut)
                    {
                        var _user = await _userManager.FindByEmailAsync(Input.Email);
                        if (_user != null)
                        {
                            //LoginAudit - Requires Verification
                            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                            var agent = UserAgentHelper.GetBrowserName(userAgent);
                            LoginAudit loginAudit = new LoginAudit();
                            loginAudit.UserId = _user.Id;
                            loginAudit.LoginStatus = Convert.ToInt32(LoginAuditStatus.LockedOut);
                            loginAudit.LoginTime = DateTime.UtcNow;
                            loginAudit.IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                            loginAudit.UserAgent = agent;
                            _uow.loginAuditRepo.Add(loginAudit);
                            _uow.SaveChanges();
                        }
                        Log.Warning("User account locked out.");
                        return RedirectToPage("./Lockout");
                    }
                    else
                    {
                        var _user = await _userManager.FindByEmailAsync(Input.Email);
                        if (_user != null)
                        {
                            //LoginAudit - Requires Verification
                            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                            var agent = UserAgentHelper.GetBrowserName(userAgent);
                            LoginAudit loginAudit = new LoginAudit();
                            loginAudit.UserId = _user.Id;
                            loginAudit.LoginStatus = Convert.ToInt32(LoginAuditStatus.Failure);
                            loginAudit.LoginTime = DateTime.UtcNow;
                            loginAudit.IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                            loginAudit.UserAgent = agent;
                            _uow.loginAuditRepo.Add(loginAudit);
                            _uow.SaveChanges();
                        }
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return Page();
                    }
                }

                return Page();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured Login :{ErrorMsg}", ex.Message);
                ModelState.AddModelError(string.Empty, "Please try after sometime.");
                return Page();
            }
            finally
            {
                _userManager.Dispose();
                recipients.Clear();
            }
        }

        public async Task<bool> TWOFA()
        {
            List<string> recipients = new List<string>();
            try
            {
                var _user = await _userManager.FindByEmailAsync(Input.Email);

                if (_user != null)
                {
                    /*if (_user.UserType == (int)UserTypes.Other)
                    {
                        if (!await _uow.loginAuditRepo.LoginValidate(_user.Id))
                        {
                            return false;
                        }
                    }*/

                    var token = await _userManager.GenerateTwoFactorTokenAsync(_user, "Email");
                    SendMail sendMail = new();
                    EmailTemplateViewModel emailTemplateViewModel = new();
                    recipients.Add(Input.Email);
                    sendMail.Recipients = recipients.ToArray();

                    emailTemplateViewModel.Otp = token;
                    emailTemplateViewModel.MailFormat = "OTP";
                    emailTemplateViewModel.TemplateFilePath = TANResource._2FAOTPEmailTemplatePath;
                    emailTemplateViewModel.Message = "OTP.";
                    emailTemplateViewModel.UserName = _user.FirstName;
                    string server = _configuration.GetValue<string>("DomainPath:Server");
                    string logoPath = _configuration.GetValue<string>("DomainPath:LogoPath");
                    emailTemplateViewModel.LogoPath = server + logoPath + "logo1.png";
                    emailTemplateViewModel.OtpPath = server + logoPath + "otp.png";
                    sendMail.Subject = "Think Anew TFA One-Time Password Verification";
                    sendMail.Body = await _mailContentHelper.CreateMailContent(emailTemplateViewModel);
                    bool isMailSent = await _emailHelper.SendEmailAsync(sendMail);
                    //LoginAudit - Requires Verification
                    var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                    var agent = UserAgentHelper.GetBrowserName(userAgent);
                    LoginAudit loginAudit = new LoginAudit();
                    loginAudit.UserId = _user.Id;
                    loginAudit.LoginStatus = Convert.ToInt32(LoginAuditStatus.Verification);
                    loginAudit.LoginTime = DateTime.UtcNow;
                    loginAudit.IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                    loginAudit.UserAgent = agent;
                    _uow.loginAuditRepo.Add(loginAudit);
                    _uow.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured Login TWOFA :{ErrorMsg}", ex.Message);
                return false;
            }
            return true;
        }

    }
}
