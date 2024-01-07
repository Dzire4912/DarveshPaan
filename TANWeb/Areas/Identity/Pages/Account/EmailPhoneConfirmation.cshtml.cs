using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TAN.DomainModels.Entities;
using TANWeb.Helpers;
using TANWeb.Interface;
using TANWeb.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace TANWeb.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    [ExcludeFromCodeCoverage]
    public class EmailPhoneConfirmationModel : PageModel
    {
        private readonly UserManager<AspNetUser> _userManager;
        private readonly ILogger<UpdatePasswordModel> _logger; 

        public EmailPhoneConfirmationModel(UserManager<AspNetUser> userManager, ILogger<UpdatePasswordModel> logger)
        {
            _userManager = userManager;
            _logger = logger; 
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public string Email { get; set; }
            [Required]
            public string? EmailOTP { get; set; }
            public bool EmailConfirmed { get; set; }
        }
        public async Task<IActionResult> OnGet(string email)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    Input = new InputModel()
                    {
                        Email = user.Email,
                        EmailConfirmed = user.EmailConfirmed
                    };
                }
                else
                {
                    return RedirectToPage("./Login");
                }
            }
            catch (Exception ex)
            {
                Log.Error("Email Confirmation get error ", ex.Message);
                return RedirectToPage("./Login");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                string returnUrl = Url.Content("~/pbjsnap/dashboard");
                if(string.IsNullOrEmpty(Input.EmailOTP) || Input.EmailOTP.Length < 6)
                {
                    ModelState.AddModelError(string.Empty, "Please enter 6 digits OTP.");
                    return Page();
                }
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null && !user.EmailConfirmed && !string.IsNullOrEmpty(Input.EmailOTP) && 
                        (HttpContext.Session.GetString("EmailOTP")?.ToString() == Input.EmailOTP))
                    {
                        user.EmailConfirmed = true;
                        HttpContext.Session.Remove("EmailOTP");
                        var result = await _userManager.UpdateAsync(user);
                        if (result.Succeeded)
                        {
                            if (user.IsUsingTempPassword == 0)
                            {
                                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                                return RedirectToPage("./UpdatePassword", new { code = code });
                            }
                            return LocalRedirect(returnUrl);
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid OTP.");
                            return Page();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid OTP.");
                        return Page();
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid OTP.");
                    return Page();
                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Please re login again!");
                Log.Error("Email Confirmation post error ", ex.Message);
            }
            finally
            {
                _userManager.Dispose();
            }
            return Page();
        }
    }
}
