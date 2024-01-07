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
using static TANWeb.Helpers.IMailHelper;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics.CodeAnalysis;

namespace TANWeb.Areas.Identity.Pages.Account
{
    [ExcludeFromCodeCoverage]
    public class UserChangePasswordModel : PageModel
    {
        private readonly UserManager<AspNetUser> _userManager;
        private readonly ILogger<UpdatePasswordModel> _logger;
        public UserChangePasswordModel(UserManager<AspNetUser> userManager, ILogger<UpdatePasswordModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }
        [BindProperty]
        public InputModel Input { get; set; }
        public class InputModel
        {
            [DataType(DataType.Password)]
            [Display(Name = "Current password")]
            public string CurrentPassword { get; set; }
            [Required(ErrorMessage = "Please enter your password")]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,30}$", ErrorMessage = "The password does not meet the password policy requirements.")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }
        public void OnGet()
        {
        }
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        var result = await _userManager.ChangePasswordAsync(user, Input.CurrentPassword, Input.Password);
                        if (result.Succeeded)
                        {
                            ModelState.AddModelError(string.Empty, "Password change successfully.");
                            return RedirectToAction("Index", "Home", new { area = ""});
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Password change failed.");
                            return Page();
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Password change failed.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            finally
            {
              //  _userManager.Dispose();
            }
            ModelState.AddModelError(string.Empty, "Password change failed.");
            return Page();
        }
    }
}
