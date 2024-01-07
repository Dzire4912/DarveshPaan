// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Serilog;
using TAN.DomainModels.Entities;
using TAN.DomainModels.ViewModels;
using TANWeb.Helpers;
using TANWeb.Interface;
using TANWeb.Models;
using TANWeb.Resources;
using static System.Net.WebRequestMethods;

namespace TANWeb.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    [ExcludeFromCodeCoverage]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<AspNetUser> _userManager;
        private readonly IMailHelper _mailHelper;
        private readonly IDataProtector _protector;
        private IMailContentHelper _mailContentHelper;
        private IConfiguration _configuration;
        private readonly IGoogleCaptcha _googleCaptcha;
        private readonly IEmailHelper _emailHelper;

        public ForgotPasswordModel(UserManager<AspNetUser> userManager, IMailHelper mailHelper, IDataProtectionProvider dataProvider, IMailContentHelper mailContentHelper, IConfiguration configuration, IGoogleCaptcha googleCaptcha, IEmailHelper emailHelper)
        {
            _userManager = userManager;
            _mailHelper = mailHelper;
            _protector = dataProvider.CreateProtector("Code");
            _mailContentHelper = mailContentHelper;
            _configuration = configuration;
            _googleCaptcha = googleCaptcha;
            _emailHelper = emailHelper;
        }


        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Please enter your email id")]
            [EmailAddress]
            public string Email { get; set; }
            [Required(ErrorMessage = "Invalid token")]
            public string Token { get; set; } = "";
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (Input.Token != null && Input.Token != "")
                    {
                        var response = await _googleCaptcha.ValidateToken(Input.Token);
                        if (!response.Success)
                        {
                            ModelState.AddModelError(string.Empty, "Invalid Token.");
                            return Page();
                        }
                    }
                    var user = await _userManager.FindByEmailAsync(Input.Email);
                    if (user == null)
                    {
                        ModelState.AddModelError(string.Empty, "Invalid email-id");
                        return Page();
                    }
                    Utils utils = new Utils();
                    SendMail sendMail = new();
                    bool isMailSent = false;
                    EmailTemplateViewModel emailTemplateVM = new();
                    var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    List<string> recipients = new List<string> { Input.Email };
                    string server = _configuration.GetValue<string>("DomainPath:Server");
                    string logoPath = _configuration.GetValue<string>("DomainPath:LogoPath");
                    if (user.IsUsingTempPassword == 0)
                    {
                        string Password = utils.CreateDummyPassword();
                        var result = await _userManager.ResetPasswordAsync(user, code, Password);
                        if (result.Succeeded)
                        {
                            sendMail.Recipients = recipients.ToArray();

                            emailTemplateVM.DummyPassword = Password;
                            emailTemplateVM.MailFormat = "Password";
                            emailTemplateVM.TemplateFilePath = TANResource.ResetPasswordEmailTemplatePath;
                            emailTemplateVM.Message = "Password.";
                            emailTemplateVM.UserName = user.FirstName;
                            emailTemplateVM.LogoPath = server + logoPath + "logo1.png";
                            emailTemplateVM.ForgotPasswordLogoPath = server + logoPath + "forgot-the-pass.png";
                            sendMail.Subject = "Think Anew Email Confirmation";
                            sendMail.Body = await _mailContentHelper.CreateMailContent(emailTemplateVM);
                            isMailSent = await _emailHelper.SendEmailAsync(sendMail);
                            if (isMailSent)
                            {
                                return RedirectToPage("./ForgotPasswordConfirmation", new { Email = _protector.Protect(Input.Email) });
                            }
                        }
                        return RedirectToPage("./ForgotPasswordConfirmation", new { Email = _protector.Protect(Input.Email) });
                    }

                    code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var callbackUrl = Url.Page(
                        "/Account/ResetPassword",
                        pageHandler: null,
                        values: new { area = "Identity", code },
                        protocol: Request.Scheme);

                    emailTemplateVM.Url = callbackUrl;
                    emailTemplateVM.MailFormat = "URL";
                    emailTemplateVM.TemplateFilePath = TANResource.ForgotPasswordEmailTemplatePath;
                    emailTemplateVM.Message = "Link.";
                    emailTemplateVM.UserName = user.FirstName;
                    emailTemplateVM.LogoPath = server + logoPath + "logo1.png";
                    emailTemplateVM.ForgotPasswordLogoPath = server + logoPath + "forgot-the-pass.png";
                    sendMail.Recipients = recipients.ToArray();
                    sendMail.Subject = "Reset Your Think Anew Reset Password";
                    sendMail.Body = await _mailContentHelper.CreateMailContent(emailTemplateVM);
                    isMailSent = await _emailHelper.SendEmailAsync(sendMail);
                    if (isMailSent)
                    {
                        return RedirectToPage("./ForgotPasswordConfirmation", new { Email = _protector.Protect(Input.Email) });
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Failed to send mail.");
                    Log.Error("", ex.ToString());
                    return Page();
                }
            }

            return Page();
        }
    }
}
