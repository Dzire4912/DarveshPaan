using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics.CodeAnalysis;
using TAN.DomainModels.Entities;
using TAN.DomainModels.ViewModels;
using TANWeb.Helpers;
using TANWeb.Interface;
using TANWeb.Models;
using TANWeb.Resources;

namespace TANWeb.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    [ExcludeFromCodeCoverage]
    public class ResendOTPModel : PageModel
    {
        private readonly SignInManager<AspNetUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<AspNetUser> _userManager;
        private Utils utils = new Utils();
        private readonly IMailHelper _mailHelper;
        private readonly IMailContentHelper _mailContentHelper;
        private readonly IConfiguration _configuration;
        private readonly IEmailHelper _emailHelper;
        public ResendOTPModel(SignInManager<AspNetUser> signInManager, ILogger<LoginModel> logger, UserManager<AspNetUser> userManager, IMailHelper mailHelper, IMailContentHelper mailContentHelper, IConfiguration configuration, IEmailHelper emailHelper)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            _mailHelper = mailHelper;
            _mailContentHelper = mailContentHelper;
            _configuration = configuration;
            _emailHelper = emailHelper;
        }
        public async Task<StatusCodeResult> OnGet(string email)
        {
            try
            {
                var _user = await _userManager.FindByEmailAsync(email);
                if (_user != null)
                {
                    var token = await _userManager.GenerateTwoFactorTokenAsync(_user, "Email");
                    List<string> recipients = new List<string> { email };
                    SendMail sendMail = new();
                    EmailTemplateViewModel emailTemplateViewModel = new();
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
                    if (isMailSent)
                    {
                        return StatusCode(200);
                    }
                    return StatusCode(200);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return StatusCode(400);
        }
    }
}
