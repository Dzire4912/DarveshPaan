
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics.CodeAnalysis;
using TAN.DomainModels.Entities;
using TAN.DomainModels.ViewModels;
using TANWeb.Helpers;
using TANWeb.Models;
using TANWeb.Resources;

namespace TANWeb.Areas.Identity.Pages.Account
{
    [ExcludeFromCodeCoverage]
    public class ResendOTPEmailConfirmedModel : PageModel
    { 
        private readonly ILogger<LoginModel> _logger;
        private readonly Microsoft.AspNetCore.Identity.UserManager<AspNetUser> _userManager;
        private Utils utils = new Utils();
        private readonly IMailContentHelper _mailContentHelper;
        private readonly IConfiguration _configuration;
        private readonly IEmailHelper _emailHelper;

        public ResendOTPEmailConfirmedModel( ILogger<LoginModel> logger,  Microsoft.AspNetCore.Identity.UserManager<AspNetUser> userManager,IMailContentHelper mailContentHelper,IConfiguration configuration,IEmailHelper emailHelper)
        {
            _logger = logger;
            _userManager = userManager;
            _mailContentHelper = mailContentHelper;
            _configuration = configuration;
            _emailHelper = emailHelper;
        }
        public async Task<StatusCodeResult> OnGet(string Email)
        {
            try
            {
                if(Email != null)
                {
                    var _user = await _userManager.FindByEmailAsync(Email);
                    if (_user != null)
                    {
                        HttpContext.Session.SetString("EmailOTP", utils.GenerateOTP());
                        SendMail sendMail = new ();
                        EmailTemplateViewModel emailTemplateViewModel= new ();
                        List<string> recipients = new List<string> { Email };
                        sendMail.Recipients = recipients.ToArray();
                        emailTemplateViewModel.Otp = HttpContext.Session.GetString("EmailOTP")?.ToString();
                        emailTemplateViewModel.MailFormat = "OTP";
                        emailTemplateViewModel.TemplateFilePath = TANResource.ConfirmAccountEmailTemplatePath;
                        emailTemplateViewModel.Message = "OTP.";
                        emailTemplateViewModel.UserName = _user.FirstName;
                        string server = _configuration.GetValue<string>("DomainPath:Server");
                        string logoPath = _configuration.GetValue<string>("DomainPath:LogoPath");
                        emailTemplateViewModel.LogoPath = server + logoPath + "logo1.png";
                        emailTemplateViewModel.OtpPath = server + logoPath + "otp.png";
                        sendMail.Subject = "Think Anew Email One-Time Password Verification";
                        sendMail.Body = await _mailContentHelper.CreateMailContent(emailTemplateViewModel);
                        bool isMailSent = await _emailHelper.SendEmailAsync(sendMail);
                        if (isMailSent)
                        {
                            return StatusCode(200);
                        }
                        return StatusCode(200);
                    }
                } 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return StatusCode(400);
        }
    }
}
