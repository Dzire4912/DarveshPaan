using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TAN.ApplicationCore;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;


namespace TAN.Repository.Implementations
{
    [ExcludeFromCodeCoverage]
    public class UploadClientFileDetailsRepository : Repository<UploadClientFileDetails>, IUploadClientFileDetailsRepository
    {
        private readonly IHttpContextAccessor _httpContext;
        private DatabaseContext Context
        {
            get
            {
                return db as DatabaseContext;
            }
        }
        public UploadClientFileDetailsRepository(DbContext db, IHttpContextAccessor httpContext)
        {
            this.db = db;
            _httpContext = httpContext;
        }

        public async Task<string> UploadFileDetailsToDB(UploadClientFileRequestViewModel request)
        {
            try
            {
                if (!string.IsNullOrEmpty(request.Result))
                {
                    var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                    var uploadClientFileDetails = new UploadClientFileDetails();
                    uploadClientFileDetails.OrganizationId = Convert.ToInt32(request.OrganizationId);
                    uploadClientFileDetails.FacilityId = Convert.ToInt32(request.FacilityId);
                    uploadClientFileDetails.CreatedDate = DateTime.UtcNow;
                    uploadClientFileDetails.IsActive = true;
                    uploadClientFileDetails.CreatedBy = userId;
                    uploadClientFileDetails.IsProcessed = 0;
                    uploadClientFileDetails.CarrierName = request.CarrierName;
                    uploadClientFileDetails.FileSize = request.FileSize;
                    if (request.IsOneDrive)
                    {
                        uploadClientFileDetails.FileName = request.FileName;
                    }
                    else
                    {
                        uploadClientFileDetails.FileName = request.UploadedFile.FileName;
                    }
                    Context.UploadClientFileDetails.Add(uploadClientFileDetails);
                    await Context.SaveChangesAsync();
                    return "success";
                }
                else
                    return "failed";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Data Service Controller / Upload File To Azure Storage :{ErrorMsg}", ex.Message);
                return "failed";
            }
        }


        public IEnumerable<UploadClientFileDetailsViewModel> GetAllUploadedFiles(int skip, int take, string? searchValue, string sortOrder, string orgName, string facilityName, string carrierName, string orgId, out int fileCount)
        {
            try
            {
                var lowercaseSearchValue = searchValue?.ToLower();
                var allFiles = (from ucfd in Context.UploadClientFileDetails
                                join f in Context.Facilities on ucfd.FacilityId equals f.Id
                                join o in Context.Organizations on ucfd.OrganizationId equals o.OrganizationID
                                where ucfd.IsActive == true && f.IsActive == true && o.IsActive == true
                                orderby ucfd.Id
                                select new UploadClientFileDetailsViewModel
                                {
                                    FacilityId = ucfd.FacilityId,
                                    OrganizationId = ucfd.OrganizationId,
                                    FileName = ucfd.FileName,
                                    CreatedDate = ucfd.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                    FacilityName = f.FacilityName,
                                    OrganizationName = o.OrganizationName,
                                    CarrierName = ucfd.CarrierName,
                                    IsProcessed = ucfd.IsProcessed,
                                    FileSize = ucfd.FileSize.ToString(),
                                }).AsEnumerable();

                if (!string.IsNullOrEmpty(orgName))
                {
                    allFiles = allFiles.Where(af => af.OrganizationName.ToLower() == orgName.ToLower()).AsEnumerable();
                }
                if (!string.IsNullOrEmpty(facilityName))
                {
                    allFiles = allFiles.Where(af => af.FacilityName.ToLower() == facilityName.ToLower()).AsEnumerable();
                }
                if (!string.IsNullOrEmpty(carrierName))
                {
                    allFiles = allFiles.Where(af => af.CarrierName.ToLower() == carrierName.ToLower()).AsEnumerable();
                }
                if (!string.IsNullOrEmpty(lowercaseSearchValue))
                {
                    allFiles = ApplySearch(allFiles, lowercaseSearchValue);
                }

                if (!string.IsNullOrEmpty(orgId))
                {
                    allFiles = allFiles.Where(a => a.OrganizationId == Convert.ToInt32(orgId)).AsEnumerable();
                }
                fileCount = allFiles.Count();

                allFiles = allFiles.Skip(skip).Take(take);


                //sort data
                if (!string.IsNullOrEmpty(sortOrder))
                {
                    switch (sortOrder)
                    {
                        case "organizationname_asc":
                            allFiles = allFiles.OrderBy(x => x.OrganizationName);
                            break;
                        case "organizationname_desc":
                            allFiles = allFiles.OrderByDescending(x => x.OrganizationName);
                            break;
                        case "facilityname_asc":
                            allFiles = allFiles.OrderBy(x => x.FacilityName);
                            break;
                        case "facilityname_desc":
                            allFiles = allFiles.OrderByDescending(x => x.FacilityName);
                            break;
                        case "carriername_asc":
                            allFiles = allFiles.OrderBy(x => x.CarrierName);
                            break;
                        case "carriername_desc":
                            allFiles = allFiles.OrderByDescending(x => x.CarrierName);
                            break;
                        case "filename_asc":
                            allFiles = allFiles.OrderBy(x => x.FileName);
                            break;
                        case "filename_desc":
                            allFiles = allFiles.OrderByDescending(x => x.FileName);
                            break;
                        case "createddate_asc":
                            allFiles = allFiles.OrderBy(x => x.FileName);
                            break;
                        case "createddate_desc":
                            allFiles = allFiles.OrderByDescending(x => x.FileName);
                            break;
                        case "filesize_asc":
                            allFiles = allFiles.OrderBy(x => x.FileSize);
                            break;
                        case "filesize_desc":
                            allFiles = allFiles.OrderByDescending(x => x.FileSize);
                            break;
                        case "isprocessed_asc":
                            allFiles = allFiles.OrderBy(x => x.IsProcessed);
                            break;
                        case "isprocessed_desc":
                            allFiles = allFiles.OrderByDescending(x => x.IsProcessed);
                            break;
                    }
                }
                return allFiles;
            }
            catch (Exception ex)
            {
                fileCount = 0;
                Log.Error(ex, "An Error Occured in Organization Controller / Viewing All Organizations :{ErrorMsg}", ex.Message);
                return Enumerable.Empty<UploadClientFileDetailsViewModel>();
            }
        }
        private IEnumerable<UploadClientFileDetailsViewModel> ApplySearch(IEnumerable<UploadClientFileDetailsViewModel> allFiles, string lowercaseSearchValue)
        {
            allFiles = allFiles.Where(af =>
                                         af.OrganizationName.ToLower().Contains(lowercaseSearchValue) ||
                                         af.FacilityName.ToLower().Contains(lowercaseSearchValue) ||
                                         af.CarrierName.ToLower().Contains(lowercaseSearchValue) ||
                                         af.FileName.ToLower().Contains(lowercaseSearchValue) ||
                                         af.FileSize.ToString().Contains(lowercaseSearchValue) ||
                                         af.CreatedDate.ToLower().Contains(lowercaseSearchValue));
            return allFiles;
        }

        public bool DeleteFile(string fileName)
        {
            try
            {
                var fileDetails = Context.UploadClientFileDetails.Where(ucfd => ucfd.FileName == fileName).FirstOrDefault();
                var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (fileDetails != null)
                {
                    fileDetails.IsActive = false;
                    fileDetails.UpdatedDate = DateTime.UtcNow;
                    fileDetails.UpdatedBy = userId;

                    Context.UploadClientFileDetails.Update(fileDetails);
                    Context.SaveChanges();
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Data Service Controller / Deleting File From Database :{ErrorMsg}", ex.Message);
                return false;
            }
        }
    }
}
