// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics.CodeAnalysis;

namespace TANWeb.Areas.Identity.Pages.Account
{
     
    [AllowAnonymous]
    [ExcludeFromCodeCoverage]
    public class ResetPasswordConfirmationModel : PageModel
    { 
        public ResetPasswordConfirmationModel()
        {
        }
        public IActionResult OnGet()
        {
            return Page();
        }

    }
}
