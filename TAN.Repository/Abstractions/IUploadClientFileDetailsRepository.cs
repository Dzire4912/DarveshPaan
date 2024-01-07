using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using TAN.DomainModels.ViewModels;

namespace TAN.Repository.Abstractions
{
    public interface IUploadClientFileDetailsRepository : IRepository<UploadClientFileDetails>
    {
        Task<string> UploadFileDetailsToDB(UploadClientFileRequestViewModel request);
        IEnumerable<UploadClientFileDetailsViewModel> GetAllUploadedFiles(int skip, int take, string? searchValue, string sortOrder, string orgName, string facilityName, string carrierName, string orgId, out int fileCount);
        bool DeleteFile(string fileName);
    }
}
