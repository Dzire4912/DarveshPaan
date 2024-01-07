using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using TAN.DomainModels.Entities;
using TANWeb.Helpers;
using TANWeb.Interface;
using TANWeb.Models;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using TANWeb.Resources;
using TAN.Repository.Abstractions;
using Microsoft.Extensions.Hosting.Internal;
using TAN.DomainModels.ViewModels;
using System.Diagnostics.CodeAnalysis;

namespace TANWeb.Areas.Identity.Pages.Account
{
    [ExcludeFromCodeCoverage]
    public class UpdatePasswordModel : PageModel
    {
        private readonly UserManager<AspNetUser> _userManager;
        private readonly ILogger<UpdatePasswordModel> _logger;
        private readonly IMailHelper _mailHelper;
        private IUnitOfWork _uow;
        private readonly IConfiguration _configuration;
        private readonly IMailContentHelper _mailContentHelper;
        private readonly IEmailHelper _emailHelper;
        public UpdatePasswordModel(UserManager<AspNetUser> userManager, ILogger<UpdatePasswordModel> logger, IMailHelper mailHelper, IUnitOfWork uow, IConfiguration configuration, IMailContentHelper mailContentHelper, IEmailHelper emailHelper)
        {
            _userManager = userManager;
            _logger = logger;
            _mailHelper = mailHelper;
            _uow = uow;
            _configuration = configuration;
            _mailContentHelper = mailContentHelper;
            _emailHelper = emailHelper;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Please enter your password")]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,30}$", ErrorMessage = "The password does not meet the password policy requirements.")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
            [Required]
            public string? Code { get; set; }
        }

        public async Task<IActionResult> OnGet(string code = null)
        {
            try
            {
                if (code == null)
                    return BadRequest("A code must be supplied for password reset."); 
                Input = new InputModel
                {
                    Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
                };
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                string returnUrl = Url.Content("~/");
                if (ModelState.IsValid)
                {
                    if (Input.Code == null)
                        return BadRequest("A code must be supplied for password reset.");

                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
                        if (result.Succeeded)
                        {
                            user.IsUsingTempPassword = 1;
                            await _userManager.UpdateAsync(user);
                            var defApp = _uow.ApplicationRepo.GetAll().Where(x => x.ApplicationId == (user.DefaultAppId)).FirstOrDefault();
                            var appName = _configuration[TANResource.PBJSnap];
                            //assign url based on default app
                            if (defApp.Name == appName)
                            {
                                returnUrl = TANResource.PBJDashboardPath;
                            }
                            else if (defApp.Name == TANResource.Inventory)
                            {
                                returnUrl = TANResource.InventoryDashboardPath;
                            }
                            Utils utils = new Utils();
                            if (!user.EmailConfirmed)
                            {
                                HttpContext.Session.SetString("EmailOTP", utils.GenerateOTP());
                                SendMail sendMail = new();
                                EmailTemplateViewModel emailTemplateViewModel = new();
                                List<string> recipients = new List<string> { user.Email };
                                sendMail.Recipients = recipients.ToArray();
                                emailTemplateViewModel.Otp = HttpContext.Session.GetString("EmailOTP")?.ToString();
                                emailTemplateViewModel.MailFormat = "OTP";
                                emailTemplateViewModel.TemplateFilePath = TANResource.ConfirmAccountEmailTemplatePath;
                                emailTemplateViewModel.Message = "OTP.";
                                emailTemplateViewModel.UserName = user.FirstName;
                                string server = _configuration.GetValue<string>("DomainPath:Server");
                                string logoPath = _configuration.GetValue<string>("DomainPath:LogoPath");
                                emailTemplateViewModel.LogoPath = server + logoPath+"logo1.png";
                                emailTemplateViewModel.OtpPath = server + logoPath+ "otp.png";
                                sendMail.Subject = "Think Anew Email One-Time Password Verification";
                                sendMail.Body = await _mailContentHelper.CreateMailContent(emailTemplateViewModel);
                                bool isMailSent = await _emailHelper.SendEmailAsync(sendMail);
                                if (isMailSent)
                                {
                                    return RedirectToPage("./EmailPhoneConfirmation");
                                }
                                return RedirectToPage("./EmailPhoneConfirmation");
                            }
                            return LocalRedirect(returnUrl);
                        }
                    }
                }
                else
                {
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            finally
            {
                _userManager.Dispose();
            }
            return Page();
        }
    }
}
