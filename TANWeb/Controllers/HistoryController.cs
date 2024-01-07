using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using static TAN.DomainModels.Helpers.Permissions;

namespace TANWeb.Controllers
{
    public class HistoryController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<AspNetUser> _userManager;

        public HistoryController(IUnitOfWork uow, UserManager<AspNetUser> userManager)
        {
            _uow = uow;
            _userManager = userManager;
        }
        [Route("/history")]
        public async Task<IActionResult> Index()
        {
            SubmissionHistoryViewModel submissionHistoryViewModel = new();
            var selectedApp = HttpContext.Session.GetString("SelectedApp");
            submissionHistoryViewModel.OrganizationList = await _uow.OrganizationRepo.GetApplicationWiseOrganizations(selectedApp);
            submissionHistoryViewModel.FacilityList = await _uow.FacilityRepo.GetAllFacilitiesByOrgId();
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            submissionHistoryViewModel.UserType = currentUser.UserType;
            return View(submissionHistoryViewModel);
        }

        public async Task<JsonResult> GetFacilities(string organizationName)
        {
            var organizationDetails = _uow.OrganizationRepo.GetAll().Where(org => org.OrganizationName == organizationName).FirstOrDefault();
            if (organizationDetails != null)
            {
                var facilityList = await _uow.FacilityRepo.GetAllFacilitiesByOrgId(organizationDetails.OrganizationID);
                return Json(facilityList);
            }
            return Json(new List<FacilityModel>());
        }


        [Authorize(Permissions.SubmissionHistory.PBJSnapView)]
        [HttpPost]
        public JsonResult GetSubmissionHistory(string orgName, string facilityName, string searchValue, string start, string length, string sortColumn, string sortDirection, string draw)
        {
            int totalRecord = 0;
            int filterRecord = 0;

            int pageSize = Convert.ToInt32(length ?? "0");
            int skip = Convert.ToInt32(start ?? "0");
            string sortOrder = string.Empty;

            IEnumerable<SubmissionHistoryViewModel> data;

            int submissionCount = 0;


            //sort data
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
            {
                sortOrder = (sortColumn + "_" + sortDirection).ToLower();
            }

            data = _uow.HistoryRepo.GetAllSubmissionHistory(skip, pageSize, searchValue, sortOrder, orgName, facilityName, out submissionCount);

            totalRecord = submissionCount;

            // get total count of records after search 
            filterRecord = totalRecord;

            var returnObj = new { draw = Convert.ToInt32(draw), recordsTotal = totalRecord, recordsFiltered = filterRecord, data = data.ToList() };
            return Json(returnObj);
        }

        [Authorize(Permissions.SubmissionHistory.PBJSnapDelete)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSubmissionHistory(string fileName)
        {
            var result = await _uow.HistoryRepo.DeleteSubmissionHistoryDetail(fileName);
            return Json(result);
        }

        [Authorize(Permissions.SubmissionHistory.PBJSnapEdit)]
        public async Task<IActionResult> GetFileDetails(string fileName)
        {
            var result = await _uow.HistoryRepo.GetDetails(fileName);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Permissions.SubmissionHistory.PBJSnapEdit)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSubmissionData(string fileId, string? fileDate, string? fileName, string? fileStatus)
        {
            var result = await _uow.HistoryRepo.UpdateSubmissionStatus(fileId, fileDate, fileName, fileStatus);
            return Json(result);
        }

        [Authorize(Permissions.SubmissionHistory.PBJSnapDownload)]
        public FileContentResult DownloadFile(string fileName)
        {
            string fileNameWithoutExtension = Path.ChangeExtension(fileName, null);
            var file = _uow.UploadHistoryRepo.GetAll().FirstOrDefault(up => up.FileName == fileNameWithoutExtension);
            if (file != null)
            {
                string xmlContent = file.UploadXMLFile;

                // Step 2: Compress the XML content into a zip file
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (ZipArchive zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        // You can adjust the file name inside the zip here.
                        var entry = zipArchive.CreateEntry(file.FileName + ".xml");

                        // Write the XML content to the zip entry
                        using (var entryStream = entry.Open())
                        using (var writer = new StreamWriter(entryStream, Encoding.UTF8))
                        {
                            writer.Write(xmlContent);
                        }
                    }

                    // Step 3: Offer the zip file for download
                    return File(memoryStream.ToArray(), "application/zip", file.FileName + ".zip");
                }
            }
            else
            {
                return File(new byte[0], "application/zip", "error.zip");
            }
        }
    }
}
