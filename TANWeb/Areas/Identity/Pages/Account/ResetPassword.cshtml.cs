// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Serilog;
using TAN.DomainModels.Entities;

namespace TANWeb.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    [ExcludeFromCodeCoverage]
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<AspNetUser> _userManager; 
        public ResetPasswordModel(UserManager<AspNetUser> userManager, IDataProtectionProvider dataProvider)
        {
            _userManager = userManager; 
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public class InputModel
        {
            [Required(ErrorMessage = "Please enter your email id")]
            [EmailAddress]
            public string Email { get; set; }
            [Required(ErrorMessage = "Please enter your password")]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,30}$", ErrorMessage = "The password does not meet the password policy requirements.")]
            [DataType(DataType.Password)]
            public string Password { get; set; }
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
            [Required]
            public string Code { get; set; }

        }

        public IActionResult OnGet(string code = null)
        {
            if (code == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }
            else
            {
                Input = new InputModel
                {
                    //Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
                    Code = code
                };
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Please enter valid email id!");
                    return Page();
                }

                var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);

                if (result.Succeeded)
                {
                    return RedirectToPage("./ResetPasswordConfirmation");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }
            catch (Exception ex)
            {
                Log.Error("", ex.ToString());

            }
            return Page();

        }
    }
}
