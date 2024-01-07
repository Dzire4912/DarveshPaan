// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;
using System.Diagnostics.CodeAnalysis;

namespace TANWeb.Areas.Identity.Pages.Account
{
     
    [AllowAnonymous]
    [ExcludeFromCodeCoverage]
    public class ForgotPasswordConfirmation : PageModel
    {
        private readonly IDataProtector _protector;
        public ForgotPasswordConfirmation(IDataProtectionProvider dataProvider)
        {
            _protector = dataProvider.CreateProtector("Code");
        }
        public IActionResult OnGet(string Email)
        {
            try
            {
                string decodedEmail = Convert.ToString(_protector.Unprotect(Email));
                TempData["Email"] = decodedEmail;
            }
            catch (Exception ex)
            {
                Log.Error("", ex.ToString());
                throw;
            }
            return Page();
        }
        
    }
}
