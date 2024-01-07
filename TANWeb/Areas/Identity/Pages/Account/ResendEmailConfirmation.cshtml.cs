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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using NuGet.Common;
using TAN.DomainModels.Entities;
using TANWeb.Helpers;
using TANWeb.Interface;
using TANWeb.Models;

namespace TANWeb.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    [ExcludeFromCodeCoverage]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<AspNetUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<UpdatePasswordModel> _logger;
        private IMailHelper _mailHelper;
        public ResendEmailConfirmationModel(UserManager<AspNetUser> userManager, IEmailSender emailSender, IMailHelper mailHelper, ILogger<UpdatePasswordModel> logger)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _mailHelper = mailHelper;
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public void OnGet()
        {
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
                    ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
                    return Page();
                }

                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { userId = userId, code = code },
                    protocol: Request.Scheme);

                //SendMail sendMail = new SendMail();
                //sendMail.Recipients = new string[] { Input.Email };
                //string value = string.Format(HtmlEncoder.Default.Encode(callbackUrl));
                //sendMail.PlaceHolders = new List<KeyValuePair<string, string>>()
                //{
                //    new KeyValuePair<string, string>("{{UserName}}", user.FirstName),
                //    new KeyValuePair<string, string>("{{Link}}",
                //        value)
                //};
                //await _mailHelper.SendEmailForEmailConfirmation(sendMail);
                ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return Page();
        }
    }
}
