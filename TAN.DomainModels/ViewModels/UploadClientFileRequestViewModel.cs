using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.ViewModels
{
    public class UploadClientFileRequestViewModel
    {
        public string? FileName { get; set; }
        public string? FileURL { get; set; }
        public bool IsOneDrive { get; set; }
        public IFormFile? UploadedFile { get; set; }
        public string? OrganizationId { get; set; }
        public string? FacilityId { get; set; }
        public string? CarrierName { get; set; }
        public string? Result { get;set; }
        public long FileSize { get; set; }
    }
}
