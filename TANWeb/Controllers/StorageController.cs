using Azure;
using Azure.Core.Serialization;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Web.Helpers;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.ViewModels;
using TANWeb.Resources;
using TANWeb.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TANWeb.Controllers
{
    public class StorageController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly StorageService _storageService;
        private readonly UserManager<AspNetUser> _userManager;
        private readonly IHttpClientFactory _httpClientFactory;

        public StorageController(IConfiguration configuration, UserManager<AspNetUser> userManager, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _userManager = userManager;
            var connectionString = _configuration.GetConnectionString("blobConn");
            _storageService = new StorageService(connectionString);
            _httpClientFactory = httpClientFactory;
        }

        [Route("storage")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Permissions.Storage.PBJSnapUpload)]
        [HttpPost]
        public async Task<IActionResult> UploadFile(StorageRequestViewModel storageRequestViewModel, IFormFile file)
        {
            string containerName;
            string fileName;
            Stream stream;
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var maxFileSizeInBytes = 100 * 1024 * 1024; // 100 MB in bytes


            if (storageRequestViewModel.IsOneDrive)
            {
                using (var httpClient = _httpClientFactory.CreateClient())
                {
                    var response = await httpClient.GetAsync(storageRequestViewModel.FileURL);

                    if (!response.IsSuccessStatusCode)
                    {
                        return Json("error");
                    }
                    
                    stream = await response.Content.ReadAsStreamAsync();
                    containerName = "user-" + currentUser.FirstName.ToLower() + "-" + currentUser.Id;
                    fileName = storageRequestViewModel.FileName;

                    // Check the file size
                    if (stream.Length > maxFileSizeInBytes)
                    {
                        return Json("sizeexceeds");
                    }
                }
            }
            else
            {
                if (file == null || file.Length == 0)
                {
                    // Handle invalid or empty file
                    return BadRequest();
                }
                else if(file.Length > maxFileSizeInBytes)
                {
                    return Json("sizeexceeds");
                }

                stream = file.OpenReadStream();
                containerName = "user-" + currentUser.FirstName.ToLower() + "-" + currentUser.Id;
                fileName = file.FileName;
            }

            // Check the file extension
            var allowedExtensions = new[] { ".xml", ".csv", ".zip", ".xls", ".xlsx" };
            var fileExtension = Path.GetExtension(fileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return Json("invalidextension");
            }
            else
            {
                var result = await _storageService.UploadFileAsync(containerName, fileName, stream);

                if (result != "exists" && result != "success")
                {
                    return Json("error");
                }

                return Json(result);
            }
        }

        [Authorize(Permissions.Storage.PBJSnapView)]
        [HttpPost]
        public async Task<JsonResult> ListFiles(string searchValue, string start, string length, string sortColumn, string sortDirection, int draw)
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            string containerName = "user-" + currentUser.FirstName.ToLower() + "-" + currentUser.Id;

            int totalRecord = 0;
            int filterRecord = 0;
            int pageSize = Convert.ToInt32(length ?? "0");
            int skip = Convert.ToInt32(start ?? "0");
            string sortOrder = string.Empty;
            IEnumerable<BlobInfoViewModel> data;
            //sort data
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
            {
                sortOrder = (sortColumn + "_" + sortDirection).ToLower();
            }

            BlobListResponseViewModel response = await _storageService.ListFilesAsync(containerName, searchValue, sortOrder, skip, pageSize);

            data = response.Files;
            totalRecord = response.TotalCount;

            // get total count of records after search 
            filterRecord = totalRecord;

            var returnObj = new { draw = draw, recordsTotal = totalRecord, recordsFiltered = filterRecord, data = data.ToList() };
            return Json(returnObj);
        }

        [Authorize(Permissions.Storage.PBJSnapDownload)]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            string containerName = "user-" + currentUser.FirstName.ToLower() + "-" + currentUser.Id;

            var stream = await _storageService.GetFileAsync(containerName, fileName);

            if (stream != null)
            {
                return File(stream, "application/octet-stream", fileName);
            }
            else
            {
                // Handle the scenario when the file is not found
                // For example, return a specific error view or a JSON response
                return NotFound("File not found.");
            }
        }

        [Authorize(Permissions.Storage.PBJSnapDelete)]
        [HttpPost]
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            string containerName = "user-" + currentUser.FirstName.ToLower() + "-" + currentUser.Id;

            bool isDeleted = await _storageService.DeleteFileAsync(containerName, fileName);

            if (isDeleted)
            {
                // File deleted successfully
                return Json(isDeleted);
            }
            else
            {
                // File or container not found
                return Json(isDeleted);
            }
        }
    }
}
