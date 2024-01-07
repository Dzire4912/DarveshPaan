using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.ViewModels
{
    public class EmailTemplateViewModel
    {
        public string? UserName { get; set; }
        public string? MailFormat { get; set; }
        public string? Otp { get; set; }
        public string? DummyPassword { get; set; }
        public string? Url { get; set; }
        public string? Message { get; set; }
        public string? TemplateFilePath { get; set; }
        public string? LogoPath { get; set; }

        public string? OtpPath { get; set; }
        public string? ForgotPasswordLogoPath { get; set; }

    }
}
