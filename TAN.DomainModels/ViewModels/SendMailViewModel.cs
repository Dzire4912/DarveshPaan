using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class SendMailViewModel
    {
        public string Recipients { get; set; }
        public string[] Bcc { get; set; }
        public string[] Cc { get; set; }

        public string Subject { get; set; }
        public string[] AttachmentFromServer { get; set; }
        public List<IFormFile> AttachmentFromUpload { get; set; }
    }
}
